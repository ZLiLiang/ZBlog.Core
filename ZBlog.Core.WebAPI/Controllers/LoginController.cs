using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using ZBlog.Core.Common.Extensions;
using ZBlog.Core.Common.GlobalVars;
using ZBlog.Core.Common.Helper;
using ZBlog.Core.Extensions.Authorization.Helpers;
using ZBlog.Core.Extensions.Authorization.Policys;
using ZBlog.Core.IServices;
using ZBlog.Core.Model;
using ZBlog.Core.Model.ViewModels;

namespace ZBlog.Core.WebAPI.Controllers
{
    /// <summary>
    /// 登录管理【无权限】
    /// </summary>
    [Produces("application/json")]
    [Route("api/Login")]
    [AllowAnonymous]
    public class LoginController : BaseApiController
    {
        private ISysUserInfoService _sysUserInfoService;
        private IUserRoleService _userRoleService;
        private IRoleService _roleService;
        private PermissionRequirement _requirement;
        private readonly IRoleModulePermissionService _roleModulePermissionService;
        private readonly ILogger<LoginController> _logger;

        /// <summary>
        /// 构造函数注入
        /// </summary>
        /// <param name="sysUserInfoService"></param>
        /// <param name="userRoleService"></param>
        /// <param name="roleService"></param>
        /// <param name="requirement"></param>
        /// <param name="roleModulePermissionService"></param>
        /// <param name="logger"></param>
        public LoginController(ISysUserInfoService sysUserInfoService,
                               IUserRoleService userRoleService,
                               IRoleService roleService,
                               PermissionRequirement requirement,
                               IRoleModulePermissionService roleModulePermissionService,
                               ILogger<LoginController> logger)
        {
            _sysUserInfoService = sysUserInfoService;
            _userRoleService = userRoleService;
            _roleService = roleService;
            _requirement = requirement;
            _roleModulePermissionService = roleModulePermissionService;
            _logger = logger;
        }

        /// <summary>
        /// 获取JWT的方法1
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pass"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Token")]
        public async Task<MessageModel<string>> GetJwtStr(string name, string pass)
        {
            string jwtStr = string.Empty;
            bool success = false;
            //这里就是用户登陆以后，通过数据库去调取数据，分配权限的操作

            var user = await _sysUserInfoService.GetUserRoleNameStr(name, MD5Helper.MD5Encrypt32(pass));
            if (user != null)
            {
                TokenModelJwt tokenModel = new TokenModelJwt
                {
                    Uid = 1,
                    Role = user
                };

                jwtStr = JwtHelper.IssueJwt(tokenModel);
                success = true;
            }
            else
            {
                jwtStr = "login fail!!!";
            }

            return new MessageModel<string>
            {
                Success = success,
                Msg = success ? "获取成功" : "获取失败",
                Response = jwtStr
            };
        }

        /// <summary>
        /// 获取JWT的方法2：给Nuxt提供
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pass"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetTokenNuxt")]
        public MessageModel<string> GetJwtStrForNuxt(string name, string pass)
        {
            string jwtStr = string.Empty;
            bool success = false;
            //这里就是用户登陆以后，通过数据库去调取数据，分配权限的操作
            //这里直接写死了
            if (name == "admins" && pass == "admins")
            {
                TokenModelJwt tokenModel = new TokenModelJwt
                {
                    Uid = 1,
                    Role = "Admin"
                };

                jwtStr = JwtHelper.IssueJwt(tokenModel);
                success = true;
            }
            else
            {
                jwtStr = "login fail!!!";
            }

            var result = new
            {
                data = new
                {
                    success = success,
                    token = jwtStr
                }
            };

            return new MessageModel<string>
            {
                Success = success,
                Msg = success ? "获取成功" : "获取失败",
                Response = jwtStr
            };
        }

        /// <summary>
        /// 获取JWT的方法3：整个系统主要方法
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pass"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("JWTToken3.0")]
        public async Task<MessageModel<TokenInfoViewModel>> GetJwtToken3(string name = "", string pass = "")
        {
            string jwtStr = string.Empty;

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(pass))
                return Failed<TokenInfoViewModel>("用户名或密码不能为空");

            pass = MD5Helper.MD5Encrypt32(pass);

            var user = await _sysUserInfoService.Query(d => d.LoginName == name && d.LoginPWD == pass && d.IsDeleted == false);
            if (user.Count > 0)
            {
                var userRoles = await _sysUserInfoService.GetUserRoleNameStr(name, pass);
                //如果是基于用户的授权策略，这里要添加用户;如果是基于角色的授权策略，这里要添加角色
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, name),
                    new Claim(JwtRegisteredClaimNames.Jti, user.FirstOrDefault().Id.ToString()),
                    new Claim("TenantId", user.FirstOrDefault().TenantId.ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTime.Now.DateToTimeStamp()),
                    new Claim(ClaimTypes.Expiration,
                        DateTime.Now.AddSeconds(_requirement.Expiration.TotalSeconds).ToString())
                };
                claims.AddRange(userRoles.Split(',').Select(s => new Claim(ClaimTypes.Role, s)));

                // ids4和jwt切换
                // jwt
                if (!Permissions.IsUseIds4)
                {
                    var data = await _roleModulePermissionService.RoleModuleMaps();
                    var list = data.Where(it => it.IsDeleted == false)
                        .OrderBy(it => it.Id)
                        .Select(it => new PermissionItem
                        {
                            Url = it.Module?.LinkUrl,
                            Role = it.Role?.Name.ObjToString()
                        })
                        .ToList();

                    _requirement.Permissions = list;
                }

                var token = JwtToken.BuildJwtToken(claims.ToArray(), _requirement);

                return Success(token, "获取成功");
            }
            else
            {
                return Failed<TokenInfoViewModel>("认证失败");
            }
        }

        [HttpGet]
        [Route("GetJwtTokenSecret")]
        public async Task<MessageModel<TokenInfoViewModel>> GetJwtTokenSecret(string name = "", string pass = "")
        {
            var rlt = await GetJwtToken3(name, pass);
            return rlt;
        }

        /// <summary>
        /// 请求刷新Token（以旧换新）
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("RefreshToken")]
        public async Task<MessageModel<TokenInfoViewModel>> RefreshToken(string token = "")
        {
            string jwtStr = string.Empty;

            if (string.IsNullOrEmpty(token))
                return Failed<TokenInfoViewModel>("token无效，请重新登录！");
            var tokenModel = JwtHelper.SerializeJwt(token);
            if (tokenModel != null && JwtHelper.CustomSafeVerify(token) && tokenModel.Uid > 0)
            {
                var user = await _sysUserInfoService.QueryById(tokenModel.Uid);
                var value = User.Claims.SingleOrDefault(s => s.Type == JwtRegisteredClaimNames.Iat)?.Value;
                if (value != null && user.CriticalModifyTime > value.ObjToDate())
                    return Failed<TokenInfoViewModel>("很抱歉,授权已失效,请重新授权！");

                if (user != null && !(value != null && user.CriticalModifyTime > value.ObjToDate()))
                {
                    var userRoles = await _sysUserInfoService.GetUserRoleNameStr(user.LoginName, user.LoginPWD);
                    //如果是基于用户的授权策略，这里要添加用户;如果是基于角色的授权策略，这里要添加角色
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.LoginName),
                        new Claim(JwtRegisteredClaimNames.Jti, tokenModel.Uid.ObjToString()),
                        new Claim(JwtRegisteredClaimNames.Iat, DateTime.Now.DateToTimeStamp()),
                        new Claim(ClaimTypes.Expiration,
                            DateTime.Now.AddSeconds(_requirement.Expiration.TotalSeconds).ToString())
                    };
                    claims.AddRange(userRoles.Split(',').Select(s => new Claim(ClaimTypes.Role, s)));

                    //用户标识
                    var identity = new ClaimsIdentity(JwtBearerDefaults.AuthenticationScheme);
                    identity.AddClaims(claims);

                    var refreshToken = JwtToken.BuildJwtToken(claims.ToArray(), _requirement);

                    return Success(refreshToken, "获取成功");
                }
            }

            return Failed<TokenInfoViewModel>("认证失败！");
        }

        /// <summary>
        /// 获取JWT的方法4：给 JSONP 测试
        /// </summary>
        /// <param name="callBack"></param>
        /// <param name="id"></param>
        /// <param name="sub"></param>
        /// <param name="expiresSliding"></param>
        /// <param name="expiresAbsoulute"></param>
        [HttpGet]
        [Route("jsonp")]
        public void Getjsonp(string callBack, long id = 1, string sub = "Admin", int expiresSliding = 30,
            int expiresAbsoulute = 30)
        {
            TokenModelJwt tokenModel = new TokenModelJwt
            {
                Uid = id,
                Role = sub
            };

            string jwtStr = JwtHelper.IssueJwt(tokenModel);

            string response = string.Format("\"value\":\"{0}\"", jwtStr);
            string call = callBack + "({" + response + "})";
            Response.WriteAsync(call);
        }

        /// <summary>
        /// 测试 MD5 加密字符串
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Md5Password")]
        public string Md5Password(string password = "")
        {
            return MD5Helper.MD5Encrypt32(password);
        }

        /// <summary>
        /// swagger登录
        /// </summary>
        /// <param name="loginRequest"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("/api/Login/swgLogin")]
        public async Task<dynamic> SwgLogin([FromBody] SwaggerLoginRequest loginRequest)
        {
            if (loginRequest is null)
            {
                return new
                {
                    result = false
                };
            }

            try
            {
                var result = await GetJwtToken3(loginRequest.Name, loginRequest.Pwd);
                if (result.Success)
                {
                    HttpContext.SuccessSwagger();
                    HttpContext.SuccessSwaggerJwt(result.Response.Token);

                    return new
                    {
                        result = true
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Swagger登录异常");
            }

            return new
            {
                result = false
            };
        }

        /// <summary>
        /// weixin登录
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("wxLogin")]
        public dynamic WxLogin(string g = "", string token = "")
        {
            return new { g, token };
        }
    }

    public class SwaggerLoginRequest
    {
        public string Name { get; set; }
        public string Pwd { get; set; }
    }
}
