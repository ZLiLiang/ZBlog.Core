using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using ZBlog.Core.Common.Extensions;
using ZBlog.Core.Common.GlobalVars;
using ZBlog.Core.Common.Helper;
using ZBlog.Core.Common.HttpContextUser;
using ZBlog.Core.IServices;
using ZBlog.Core.Model;
using ZBlog.Core.Model.CustomEnums;
using ZBlog.Core.Model.Models;

namespace ZBlog.Core.Extensions.Authorization.Policys
{
    /// <summary>
    /// 权限授权处理器
    /// </summary>
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        /// <summary>
        /// 验证方案提供对象
        /// </summary>
        public IAuthenticationSchemeProvider Schemes { get; set; }

        private readonly IRoleModulePermissionService _roleModulePermissionService;
        private readonly IHttpContextAccessor _accessor;
        private readonly ISysUserInfoService _userService;
        private readonly IUser _user;

        /// <summary>
        /// 构造函数注入
        /// </summary>
        /// <param name="schemes"></param>
        /// <param name="roleModulePermissionService"></param>
        /// <param name="accessor"></param>
        /// <param name="userService"></param>
        /// <param name="user"></param>
        public PermissionHandler(IAuthenticationSchemeProvider schemes, IRoleModulePermissionService roleModulePermissionService, IHttpContextAccessor accessor, ISysUserInfoService userService, IUser user)
        {
            Schemes = schemes;
            _roleModulePermissionService = roleModulePermissionService;
            _accessor = accessor;
            _userService = userService;
            _user = user;
        }

        /// <summary>
        /// 重写异步处理程序
        /// </summary>
        /// <param name="context"></param>
        /// <param name="requirement"></param>
        /// <returns></returns>
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            var httpContext = _accessor.HttpContext;

            // 获取系统中所有的角色和菜单的关系集合
            if (!requirement.Permissions.Any())
            {
                var data = await _roleModulePermissionService.RoleModuleMaps();
                var list = new List<PermissionItem>();
                // ids4和jwt切换
                // ids4
                if (Permissions.IsUseIds4)
                {
                    list = data.Where(it => it.IsDeleted == false)
                            .OrderBy(it => it.Id)
                            .Select(it => new PermissionItem
                            {
                                Url = it.Module?.LinkUrl,
                                Role = it.Role?.Id.ObjToString()
                            })
                            .ToList();
                }
                // jwt
                else
                {
                    list = data.Where(it => it.IsDeleted == false)
                            .OrderBy(it => it.Id)
                            .Select(it => new PermissionItem
                            {
                                Url = it.Module?.LinkUrl,
                                Role = it.Role?.Name.ObjToString()
                            })
                            .ToList();
                }

                requirement.Permissions = list;
            }

            if (httpContext != null)
            {
                var questUrl = httpContext.Request.Path.Value.ToLower();

                // 整体结构类似认证中间件UseAuthentication的逻辑，具体查看开源地址
                // https://github.com/dotnet/aspnetcore/blob/master/src/Security/Authentication/Core/src/AuthenticationMiddleware.cs
                httpContext.Features.Set<IAuthenticationFeature>(new AuthenticationFeature
                {
                    OriginalPath = httpContext.Request.Path,
                    OriginalPathBase = httpContext.Request.PathBase
                });

                // Give any IAuthenticationRequestHandler schemes a chance to handle the request
                // 主要作用是: 判断当前是否需要进行远程验证，如果是就进行远程验证
                var handlers = httpContext.RequestServices.GetRequiredService<IAuthenticationHandlerProvider>();
                foreach (var scheme in await Schemes.GetRequestHandlerSchemesAsync())
                {
                    if (await handlers.GetHandlerAsync(httpContext, scheme.Name) is IAuthenticationRequestHandler handler && await handler.HandleRequestAsync())
                    {
                        context.Fail();
                        return;
                    }
                }

                //判断请求是否拥有凭据，即有没有登录
                var defaultAuthenticate = await Schemes.GetDefaultAuthenticateSchemeAsync();
                if (defaultAuthenticate != null)
                {
                    var result = await httpContext.AuthenticateAsync(defaultAuthenticate.Name);

                    // 是否开启测试环境
                    var isTestCurrent = AppSettings.App(new string[] { "AppSettings", "UseLoadTest" }).ObjToBool();

                    //result?.Principal不为空即登录成功
                    if (result?.Principal != null || isTestCurrent || httpContext.IsSuccessSwagger())
                    {
                        if (!isTestCurrent)
                            httpContext.User = result.Principal;

                        // 应该要先校验用户的信息 再校验菜单权限相关的
                        // JWT模式下校验当前用户状态
                        // IDS4也可以校验，可以通过服务或者接口形式
                        SysUserInfo user = new SysUserInfo();
                        if (!Permissions.IsUseIds4)
                        {
                            //校验用户
                            user = await _userService.QueryById(_user.ID, true);
                            if (user == null)
                            {
                                _user.MessageModel = new ApiResponse(StatusCode.CODE401, "用户不存在或已被删除").MessageModel;
                                context.Fail(new AuthorizationFailureReason(this, _user.MessageModel.Msg));
                                return;
                            }

                            if (user.IsDeleted)
                            {
                                _user.MessageModel = new ApiResponse(StatusCode.CODE401, "用户已被删除,禁止登陆!").MessageModel;
                                context.Fail(new AuthorizationFailureReason(this, _user.MessageModel.Msg));
                                return;
                            }

                            if (!user.Enable)
                            {
                                _user.MessageModel = new ApiResponse(StatusCode.CODE401, "用户已被禁用!禁止登陆!").MessageModel;
                                context.Fail(new AuthorizationFailureReason(this, _user.MessageModel.Msg));
                                return;
                            }
                        }

                        // 判断token是否过期，过期则重新登录
                        var isExp = false;
                        // ids4和jwt切换
                        // ids4
                        if (Permissions.IsUseIds4)
                        {
                            isExp = (httpContext.User.Claims.FirstOrDefault(it => it.Type == "exp")?.Value) != null &&
                                DateHelper.StampToDateTime(httpContext.User.Claims.FirstOrDefault(it => it.Type == "exp")?.Value) >= DateTime.Now;
                        }
                        // jwt
                        else
                        {
                            isExp = (httpContext.User.Claims.FirstOrDefault(it => it.Type == ClaimTypes.Expiration)?.Value) != null &&
                                DateTime.Parse(httpContext.User.Claims.FirstOrDefault(it => it.Type == ClaimTypes.Expiration)?.Value) >= DateTime.Now;
                        }

                        if (!isExp)
                        {
                            context.Fail(new AuthorizationFailureReason(this, "授权已过期,请重新授权"));
                            return;
                        }

                        //校验签发时间
                        if (!Permissions.IsUseIds4)
                        {
                            var value = httpContext.User.Claims.FirstOrDefault(it => it.Type == JwtRegisteredClaimNames.Iat)?.Value;
                            if (value != null)
                            {
                                if (user.CriticalModifyTime > value.ObjToDate())
                                {
                                    _user.MessageModel = new ApiResponse(StatusCode.CODE401, "很抱歉,授权已失效,请重新授权").MessageModel;
                                    context.Fail(new AuthorizationFailureReason(this, _user.MessageModel.Msg));
                                    return;
                                }
                            }
                        }

                        // 获取当前用户的角色信息
                        var currentUserRoles = httpContext.User.Claims.Where(it => it.Type == ClaimTypes.Role)
                                .Select(it => it.Value)
                                .ToList();

                        if (!currentUserRoles.Any())
                        {
                            currentUserRoles = httpContext.User.Claims.Where(it => it.Type == "role")
                               .Select(it => it.Value)
                               .ToList();
                        }

                        //超级管理员 默认拥有所有权限
                        if (currentUserRoles.All(it => it != "SuperAdmin"))
                        {
                            var isMatchRole = false;
                            var permisssionRoles = requirement.Permissions.Where(it => currentUserRoles.Contains(it.Role));
                            foreach (var item in permisssionRoles)
                            {
                                try
                                {
                                    if (Regex.Match(questUrl, item.Url?.ObjToString().ToLower())?.Value == questUrl)
                                    {
                                        isMatchRole = true;
                                        break;
                                    }
                                }
                                catch (Exception ex)
                                {

                                }
                            }

                            //验证权限
                            if (currentUserRoles.Count <= 0 || !isMatchRole)
                            {
                                context.Fail();
                                return;
                            }
                        }

                        context.Succeed(requirement);
                        return;
                    }
                }

                //判断没有登录时，是否访问登录的url,并且是Post请求，并且是form表单提交类型，否则为失败
                if (!(questUrl.Equals(requirement.LoginPath.ToLower(), StringComparison.Ordinal) &&
                      (!httpContext.Request.Method.Equals("POST") || !httpContext.Request.HasFormContentType)))
                {
                    context.Fail();
                    return;
                }
            }
        }
    }
}
