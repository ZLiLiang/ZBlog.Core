using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZBlog.Core.Common.GlobalVars;
using ZBlog.Core.Common.HttpContextUser;
using ZBlog.Core.Extensions.Authorization.Policys;
using ZBlog.Core.IServices;
using ZBlog.Core.Model.Models;
using ZBlog.Core.Model;
using ZBlog.Core.Repository.UnitOfWorks;
using ZBlog.Core.Common.Helper;
using ZBlog.Core.Services;
using System.Security.Claims;
using ZBlog.Core.Extensions.Authorization.Helpers;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace ZBlog.Core.WebAPI.Controllers
{
    /// <summary>
    /// 菜单管理
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Permissions.Name)]
    public class PermissionController : BaseApiController
    {
        private readonly IUnitOfWorkManage _unitOfWorkManage;
        private readonly IPermissionService _permissionService;
        private readonly IModuleService _moduleService;
        private readonly IRoleModulePermissionService _roleModulePermissionService;
        private readonly IUserRoleService _userRoleService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUser _user;
        private readonly PermissionRequirement _requirement;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="unitOfWorkManage"></param>
        /// <param name="permissionService"></param>
        /// <param name="moduleService"></param>
        /// <param name="roleModulePermissionService"></param>
        /// <param name="userRoleService"></param>
        /// <param name="httpClientFactory"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="user"></param>
        /// <param name="requirement"></param>
        public PermissionController(IUnitOfWorkManage unitOfWorkManage,
                                    IPermissionService permissionService,
                                    IModuleService moduleService,
                                    IRoleModulePermissionService roleModulePermissionService,
                                    IUserRoleService userRoleService,
                                    IHttpClientFactory httpClientFactory,
                                    IHttpContextAccessor httpContextAccessor,
                                    IUser user,
                                    PermissionRequirement requirement)
        {
            _unitOfWorkManage = unitOfWorkManage;
            _permissionService = permissionService;
            _moduleService = moduleService;
            _roleModulePermissionService = roleModulePermissionService;
            _userRoleService = userRoleService;
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _user = user;
            _requirement = requirement;
        }



        /// <summary>
        /// 获取菜单
        /// </summary>
        /// <param name="page"></param>
        /// <param name="key"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<PageModel<Permission>>> Get(int page = 1, string key = "", int pageSize = 50)
        {
            PageModel<Permission> permissions = new PageModel<Permission>();
            if (string.IsNullOrEmpty(key) || string.IsNullOrWhiteSpace(key))
            {
                key = "";
            }

            permissions = await _permissionService.QueryPage(a => a.IsDeleted != true && (a.Name != null && a.Name.Contains(key)), page, pageSize, " Id desc ");

            #region 单独处理

            var apis = await _moduleService.Query(d => d.IsDeleted == false);
            var permissionsView = permissions.Data;

            var permissionAll = await _permissionService.Query(d => d.IsDeleted != true);
            foreach (var item in permissionsView)
            {
                List<long> pidArr = new()
                {
                    item.Pid
                };
                if (item.Pid > 0)
                {
                    pidArr.Add(0);
                }
                var parent = permissionAll.FirstOrDefault(d => d.Id == item.Pid);

                while (parent != null)
                {
                    pidArr.Add(parent.Id);
                    parent = permissionAll.FirstOrDefault(d => d.Id == parent.Pid);
                }

                item.PidArr = pidArr.OrderBy(d => d)
                    .Distinct()
                    .ToList();
                foreach (var pid in item.PidArr)
                {
                    var per = permissionAll.FirstOrDefault(d => d.Id == pid);
                    item.PNameArr.Add((per != null ? per.Name : "根节点") + "/");
                }

                item.MName = apis.FirstOrDefault(d => d.Id == item.Mid)?.LinkUrl;
            }

            permissions.Data = permissionsView;

            #endregion

            return permissions.DataCount >= 0 ? Success(permissions, "获取成功") : Failed<PageModel<Permission>>("获取失败");
        }

        /// <summary>
        /// 查询树形 Table
        /// </summary>
        /// <param name="father">父节点</param>
        /// <param name="key">关键字</param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<MessageModel<List<Permission>>> GetTreeTable(long father = 0, string key = "")
        {
            List<Permission> permissions = new List<Permission>();
            var apiList = await _moduleService.Query(d => d.IsDeleted == false);
            var permissionsList = await _permissionService.Query(d => d.IsDeleted == false);
            if (string.IsNullOrEmpty(key) || string.IsNullOrWhiteSpace(key))
            {
                key = "";
            }

            if (key != "")
            {
                permissions = permissionsList.Where(a => a.Name.Contains(key))
                    .OrderBy(a => a.OrderSort)
                    .ToList();
            }
            else
            {
                permissions = permissionsList.Where(a => a.Pid == father)
                    .OrderBy(a => a.OrderSort)
                    .ToList();
            }

            foreach (var item in permissions)
            {
                List<long> pidArr = new() { };
                var parent = permissionsList.FirstOrDefault(d => d.Id == item.Pid);

                while (parent != null)
                {
                    pidArr.Add(parent.Id);
                    parent = permissionsList.FirstOrDefault(d => d.Id == parent.Pid);
                }

                pidArr.Reverse();
                pidArr.Insert(0, 0);
                item.PidArr = pidArr;

                item.MName = apiList.FirstOrDefault(d => d.Id == item.Mid)?.LinkUrl;
                item.HasChildren = permissionsList.Where(d => d.Pid == item.Id).Any();
            }

            return Success(permissions, "获取成功");
        }

        /// <summary>
        /// 添加一个菜单
        /// </summary>
        /// <param name="permission"></param>
        /// <returns></returns>
        // POST: api/User
        [HttpPost]
        public async Task<MessageModel<string>> Post([FromBody] Permission permission)
        {
            permission.CreateId = _user.ID;
            permission.CreateBy = _user.Name;

            var id = (await _permissionService.Add(permission));

            return id > 0 ? Success(id.ObjToString(), "添加成功") : Failed("添加失败");
        }

        /// <summary>
        /// 保存菜单权限分配
        /// </summary>
        /// <param name="assignView"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MessageModel<string>> Assign([FromBody] AssignView assignView)
        {
            if (assignView.Rid > 0)
            {
                //开启事务
                try
                {
                    var old_rmps = await _roleModulePermissionService.Query(d => d.RoleId == assignView.Rid);

                    _unitOfWorkManage.BeginTran();

                    await _permissionService.Db.Deleteable<RoleModulePermission>(it => it.RoleId == assignView.Rid).ExecuteCommandAsync();
                    var permissions = await _permissionService.Query(it => it.IsDeleted == false);

                    List<RoleModulePermission> new_rmps = new List<RoleModulePermission>();
                    var nowTime = _permissionService.Db.GetDate();
                    foreach (var item in assignView.Pids)
                    {
                        var moduleId = permissions.Find(p => p.Id == item)?.Mid;
                        var find_old_rmps = old_rmps.Find(p => p.PermissionId == item);

                        RoleModulePermission roleModulePermission = new RoleModulePermission
                        {
                            IsDeleted = false,
                            RoleId = assignView.Rid,
                            ModuleId = moduleId.ObjToLong(),
                            PermissionId = item,
                            CreateId = find_old_rmps == null ? _user.ID : find_old_rmps.CreateId,
                            CreateBy = find_old_rmps == null ? _user.Name : find_old_rmps.CreateBy,
                            CreateTime = find_old_rmps == null ? nowTime : find_old_rmps.CreateTime,
                            ModifyId = _user.ID,
                            ModifyBy = _user.Name,
                            ModifyTime = nowTime
                        };
                        new_rmps.Add(roleModulePermission);
                    }
                    if (new_rmps.Count > 0)
                        await _roleModulePermissionService.Add(new_rmps);

                    _unitOfWorkManage.CommitTran();
                }
                catch (Exception)
                {
                    _unitOfWorkManage.RollbackTran();
                    throw;
                }
                _requirement.Permissions.Clear();

                return Success<string>("保存成功");
            }
            else
            {
                return Failed<string>("请选择要操作的角色");
            }
        }

        /// <summary>
        /// 获取菜单树
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="needbtn"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<PermissionTree>> GetPermissionTree(long pid = 0, bool needbtn = false)
        {
            var permissions = await _permissionService.Query(d => d.IsDeleted == false);
            var permissionTrees = permissions.Where(it => it.IsDeleted == false)
                .OrderBy(it => it.Id)
                .Select(it => new PermissionTree
                {
                    Value = it.Id,
                    Label = it.Name,
                    PId = it.Pid,
                    IsBtn = it.IsButton,
                    Order = it.OrderSort
                })
                .OrderBy(it => it.Order)
                .ToList();

            PermissionTree rootRoot = new PermissionTree
            {
                Value = 0,
                PId = 0,
                Label = "根节点"
            };

            RecursionHelper.LoopToAppendChildren(permissionTrees, rootRoot, pid, needbtn);

            return Success(rootRoot, "获取成功");
        }

        /// <summary>
        /// 获取路由树
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<NavigationBar>> GetNavigationBar(long uid)
        {
            var data = new MessageModel<NavigationBar>();

            long uidInHttpcontext = 0;
            var roleIds = new List<long>();

            // ids4和jwt切换
            if (Permissions.IsUseIds4)
            {
                // ids4
                uidInHttpcontext = _httpContextAccessor.HttpContext.User.Claims
                    .Where(it => it.Type == ClaimTypes.NameIdentifier)
                    .Select(it => it.Value)
                    .FirstOrDefault()
                    .ObjToLong();
                if (!(uidInHttpcontext > 0))
                {
                    uidInHttpcontext = _httpContextAccessor.HttpContext.User.Claims
                        .Where(it => it.Type == "sub")
                        .Select(it => it.Value)
                        .FirstOrDefault()
                        .ObjToLong();
                }
                roleIds = _httpContextAccessor.HttpContext.User.Claims
                    .Where(it => it.Type == ClaimTypes.Role)
                    .Select(it => it.Value.ObjToLong())
                    .ToList();
                if (!roleIds.Any())
                {
                    roleIds = _httpContextAccessor.HttpContext.User.Claims
                        .Where(it => it.Type == "role")
                        .Select(it => it.Value.ObjToLong())
                        .ToList();
                }
            }
            else
            {
                // jwt
                uidInHttpcontext = (JwtHelper.SerializeJwt(_httpContextAccessor.HttpContext.Request.Headers["Authorization"].ObjToString().Replace("Bearer ", ""))?.Uid).ObjToLong();
                roleIds = (await _userRoleService.Query(d => d.IsDeleted == false && d.UserId == uid))
                    .Select(d => d.RoleId.ObjToLong())
                    .Distinct()
                    .ToList();
            }

            if (uid > 0 && uid == uidInHttpcontext)
            {
                if (roleIds.Any())
                {
                    var pids = (await _roleModulePermissionService.Query(d => d.IsDeleted == false && roleIds.Contains(d.RoleId)))
                        .Select(d => d.PermissionId.ObjToLong())
                        .Distinct();
                    if (pids.Any())
                    {
                        var rolePermissionMoudles = (await _permissionService.Query(d => pids.Contains(d.Id)))
                            .OrderBy(c => c.OrderSort);
                        var temp = rolePermissionMoudles.ToList()
                            .Find(it => it.Id == 87);
                        var permissionTrees = rolePermissionMoudles.Where(it => it.IsDeleted == false)
                            .OrderBy(it => it.Id)
                            .Select(it => new NavigationBar
                            {
                                Id = it.Id,
                                Name = it.Name,
                                PId = it.Pid,
                                Order = it.OrderSort,
                                Path = it.Code,
                                IconCls = it.Icon,
                                Func = it.Func,
                                IsHide = it.IsHide.ObjToBool(),
                                IsButton = it.IsButton.ObjToBool(),
                                Meta = new NavigationBarMeta
                                {
                                    RequireAuth = true,
                                    Title = it.Name,
                                    NoTabPage = it.IsHide.ObjToBool(),
                                    KeepAlive = it.IskeepAlive.ObjToBool()
                                }
                            })
                            .OrderBy(it => it.Order)
                            .ToList();

                        NavigationBar rootRoot = new NavigationBar
                        {
                            Id = 0,
                            PId = 0,
                            Order = 0,
                            Name = "根节点",
                            Path = "",
                            IconCls = "",
                            Meta = new NavigationBarMeta()
                        };

                        RecursionHelper.LoopNaviBarAppendChildren(permissionTrees, rootRoot);

                        data.Success = true;
                        data.Response = rootRoot;
                        data.Msg = "获取成功";
                    }
                }
            }

            return data;
        }

        /// <summary>
        /// 获取路由树
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<List<NavigationBarPro>>> GetNavigationBarPro(long uid)
        {
            var data = new MessageModel<List<NavigationBarPro>>();

            long uidInHttpcontext = 0;
            var roleIds = new List<long>();

            // ids4和jwt切换
            if (Permissions.IsUseIds4)
            {
                // ids4
                uidInHttpcontext = _httpContextAccessor.HttpContext.User.Claims
                    .Where(it => it.Type == ClaimTypes.NameIdentifier)
                    .Select(it => it.Value)
                    .FirstOrDefault()
                    .ObjToLong();
                if (!(uidInHttpcontext > 0))
                {
                    uidInHttpcontext = _httpContextAccessor.HttpContext.User.Claims
                        .Where(it => it.Type == "sub")
                        .Select(it => it.Value)
                        .FirstOrDefault()
                        .ObjToLong();
                }
                roleIds = _httpContextAccessor.HttpContext.User.Claims
                    .Where(it => it.Type == ClaimTypes.Role)
                    .Select(it => it.Value.ObjToLong())
                    .ToList();
                if (!roleIds.Any())
                {
                    roleIds = _httpContextAccessor.HttpContext.User.Claims
                        .Where(it => it.Type == "role")
                        .Select(it => it.Value.ObjToLong())
                        .ToList();
                }
            }
            else
            {
                // jwt
                uidInHttpcontext = (JwtHelper.SerializeJwt(_httpContextAccessor.HttpContext.Request.Headers.Authorization.ObjToString().Replace("Bearer ", ""))?.Uid).ObjToLong();
                roleIds = (await _userRoleService.Query(d => d.IsDeleted == false && d.UserId == uid))
                    .Select(d => d.RoleId.ObjToLong())
                    .Distinct()
                    .ToList();
            }

            if (uid > 0 && uid == uidInHttpcontext)
            {
                if (roleIds.Any())
                {
                    var pids = (await _roleModulePermissionService.Query(d => d.IsDeleted == false && roleIds.Contains(d.RoleId)))
                        .Select(d => d.PermissionId.ObjToLong())
                        .Distinct();
                    if (pids.Any())
                    {
                        var rolePermissionMoudles = (await _permissionService.Query(d => pids.Contains(d.Id) && d.IsButton == false))
                            .OrderBy(c => c.OrderSort);
                        var permissionTrees = rolePermissionMoudles.Where(it => it.IsDeleted == false)
                            .OrderBy(it => it.Id)
                            .Select(it => new NavigationBarPro
                            {
                                Id = it.Id,
                                Name = it.Name,
                                ParentId = it.Pid,
                                Order = it.OrderSort,
                                Path = it.Code == "-" ? it.Name.GetTotalPingYin().FirstOrDefault() : (it.Code == "/" ? "/dashboard/workplace" : it.Code),
                                Component = it.Pid == 0 ? (it.Code == "/" ? "dashboard/Workplace" : "RouteView") : it.Code?.TrimStart('/'),
                                IconCls = it.Icon,
                                Func = it.Func,
                                IsHide = it.IsHide.ObjToBool(),
                                IsButton = it.IsButton.ObjToBool(),
                                Meta = new NavigationBarMetaPro
                                {
                                    Show = true,
                                    Title = it.Name,
                                    Icon = "user"//item.Icon
                                }
                            })
                            .OrderBy(it => it.Order)
                            .ToList();

                        data.Success = true;
                        data.Response = permissionTrees;
                        data.Msg = "获取成功";
                    }
                }
            }

            return data;
        }

        /// <summary>
        /// 通过角色获取菜单
        /// </summary>
        /// <param name="rid"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<MessageModel<AssignShow>> GetPermissionIdByRoleId(long rid = 0)
        {
            var rmps = await _roleModulePermissionService.Query(d => d.IsDeleted == false && d.RoleId == rid);
            var permissionTrees = rmps.OrderBy(it => it.Id)
                    .Select(it => it.PermissionId.ObjToLong())
                    .ToList();

            var permissions = await _permissionService.Query(it => it.IsDeleted == false);
            List<string> assignbtns = new List<string>();

            foreach (var item in permissionTrees)
            {
                var pername = permissions.FirstOrDefault(it => it.IsButton && it.Id == item)?.Name;
                if (!string.IsNullOrEmpty(pername))
                {
                    //assignbtns.Add(pername + "_" + item);
                    assignbtns.Add(item.ObjToString());
                }
            }

            return Success(new AssignShow
            {
                Permissionids = permissionTrees,
                Assignbtns = assignbtns
            }, "获取成功");
        }

        /// <summary>
        /// 更新菜单
        /// </summary>
        /// <param name="permission"></param>
        /// <returns></returns>
        // PUT: api/User/5
        [HttpPut]
        public async Task<MessageModel<string>> Put([FromBody] Permission permission)
        {
            var data = new MessageModel<string>();
            if (permission != null && permission.Id > 0)
            {
                data.Success = await _permissionService.Update(permission);
                await _roleModulePermissionService.UpdateModuleId(permission.Id, permission.Mid);
                if (data.Success)
                {
                    data.Msg = "更新成功";
                    data.Response = permission?.Id.ObjToString();
                }
            }

            return data;
        }

        /// <summary>
        ///  删除菜单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // DELETE: api/ApiWithActions/5
        [HttpDelete]
        public async Task<MessageModel<string>> Delete(long id)
        {
            var data = new MessageModel<string>();
            if (id > 0)
            {
                var userDetail = await _permissionService.QueryById(id);
                userDetail.IsDeleted = true;
                data.Success = await _permissionService.Update(userDetail);
                if (data.Success)
                {
                    data.Msg = "删除成功";
                    data.Response = userDetail?.Id.ObjToString();
                }
            }

            return data;
        }

        /// <summary>
        /// 导入多条菜单信息
        /// </summary>
        /// <param name="permissions"></param>
        /// <returns></returns>
        // POST: api/User
        [HttpPost]
        public async Task<MessageModel<string>> BatchPost([FromBody] List<Permission> permissions)
        {
            var data = new MessageModel<string>();
            string ids = string.Empty;
            int successCount = 0;

            for (int i = 0; i < permissions.Count; i++)
            {
                var permission = permissions[i];
                if (permission != null)
                {
                    permission.CreateId = _user.ID;
                    permission.CreateBy = _user.Name;
                    ids += (await _permissionService.Add(permission));
                    successCount++;
                }
            }

            data.Success = ids.IsNotEmptyOrNull();
            if (data.Success)
            {
                data.Response = ids;
                data.Msg = $"{successCount}条数据添加成功";
            }

            return data;
        }

        /// <summary>
        /// 系统接口菜单同步接口
        /// </summary>
        /// <param name="action"></param>
        /// <param name="token"></param>
        /// <param name="gatewayPrefix"></param>
        /// <param name="swaggerDomain"></param>
        /// <param name="controllerName"></param>
        /// <param name="pid"></param>
        /// <param name="isAction"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<List<Permission>>> MigratePermission(string action = "",
                                                                            string token = "",
                                                                            string gatewayPrefix = "",
                                                                            string swaggerDomain = "",
                                                                            string controllerName = "",
                                                                            long pid = 0,
                                                                            bool isAction = false)
        {
            var data = new MessageModel<List<Permission>>();
            if (controllerName.IsNullOrEmpty())
            {
                data.Msg = "必须填写要迁移的所属接口的控制器名称";
                return data;
            }

            controllerName = controllerName.TrimEnd('/').ToLower();

            gatewayPrefix = gatewayPrefix.Trim();
            swaggerDomain = swaggerDomain.Trim();
            controllerName = controllerName.Trim();

            using var client = _httpClientFactory.CreateClient();
            var configuration = swaggerDomain.IsNotEmptyOrNull() ? swaggerDomain : AppSettings.GetValue("SystemCfg:Domain");
            var url = $"{configuration}/swagger/V2/swagger.json";
            if (configuration.IsNullOrEmpty())
            {
                data.Msg = "Swagger.json在线文件域名不能为空";
                return data;
            }
            if (token.IsNullOrEmpty())
                token = Request.Headers.Authorization;
            token = token.Trim();
            client.DefaultRequestHeaders.Add("Authorization", $"{token}");

            var response = await client.GetAsync(url);
            var body = await response.Content.ReadAsStringAsync();

            var resultJObj = (JObject)JsonConvert.DeserializeObject(body);
            var paths = resultJObj["paths"].ObjToString();
            var pathsJObj = (JObject)JsonConvert.DeserializeObject(paths);

            List<Permission> permissions = new List<Permission>();
            foreach (JProperty jProperty in pathsJObj.Properties())
            {
                var apiPath = gatewayPrefix + jProperty.Name.ToLower();
                if (action.IsNotEmptyOrNull())
                {
                    action = action.Trim();
                    if (!apiPath.Contains(action.ToLower()))
                    {
                        continue;
                    }
                }
                string httpmethod = "";
                if (jProperty.Value.ToString().ToLower().Contains("get"))
                {
                    httpmethod = "get";
                }
                else if (jProperty.Value.ToString().ToLower().Contains("post"))
                {
                    httpmethod = "post";
                }
                else if (jProperty.Value.ToString().ToLower().Contains("put"))
                {
                    httpmethod = "put";
                }
                else if (jProperty.Value.ToString().ToLower().Contains("delete"))
                {
                    httpmethod = "delete";
                }

                var summary = jProperty.Value?.SelectToken($"{httpmethod}.summary")?.ObjToString() ?? "";

                var subIx = summary.IndexOf("(Auth");
                if (subIx >= 0)
                {
                    summary = summary.Substring(0, subIx);
                }

                permissions.Add(new Permission
                {
                    Code = " ",
                    Name = summary,
                    IsButton = true,
                    IsHide = false,
                    Enabled = true,
                    CreateTime = DateTime.Now,
                    IsDeleted = false,
                    Pid = pid,
                    Module = new Modules()
                    {
                        LinkUrl = apiPath ?? "",
                        Name = summary,
                        Enabled = true,
                        CreateTime = DateTime.Now,
                        ModifyTime = DateTime.Now,
                        IsDeleted = false,
                    }
                });
            }

            var modulesList = (await _moduleService.Query(d => d.IsDeleted == false && d.LinkUrl != null))
                .Select(d => d.LinkUrl.ToLower())
                .ToList();
            permissions = permissions.Where(d => !modulesList.Contains(d.Module.LinkUrl.ToLower()) && d.Module.LinkUrl.Contains($"/{controllerName}/"))
                .ToList();

            if (isAction)
            {
                foreach (var item in permissions)
                {
                    List<Modules> modules = await _moduleService.Query(d => d.LinkUrl != null && d.LinkUrl.ToLower() == item.Module.LinkUrl);
                    if (!modules.Any())
                    {
                        var mid = await _moduleService.Add(item.Module);
                        if (mid > 0)
                        {
                            item.Mid = mid;
                            var permissionid = await _permissionService.Add(item);
                        }
                    }
                }
                data.Msg = "同步完成";
            }

            data.Response = permissions;
            data.Status = 200;
            data.Success = isAction;

            return data;
        }
    }

    public class AssignView
    {
        public List<long> Pids { get; set; }
        public long Rid { get; set; }
    }
    public class AssignShow
    {
        public List<long> Permissionids { get; set; }
        public List<string> Assignbtns { get; set; }
    }
}
