using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZBlog.Core.Common.GlobalVars;
using ZBlog.Core.Common.HttpContextUser;
using ZBlog.Core.IServices;
using ZBlog.Core.Model.ViewModels;
using ZBlog.Core.Model;
using ZBlog.Core.Repository.UnitOfWorks;
using ZBlog.Core.Services;
using ZBlog.Core.Model.Models;
using ZBlog.Core.Common.Helper;
using ZBlog.Core.Extensions.Authorization.Helpers;

namespace ZBlog.Core.WebAPI.Controllers
{
    /// <summary>
    /// 用户管理
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Permissions.Name)]
    public class UserController : BaseApiController
    {
        private readonly IUnitOfWorkManage _unitOfWorkManage;
        private readonly ISysUserInfoService _sysUserInfoService;
        private readonly IUserRoleService _userRoleService;
        private readonly IRoleService _roleService;
        private readonly IDepartmentService _departmentService;
        private readonly IUser _user;
        private readonly IMapper _mapper;
        private readonly ILogger<UserController> _logger;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="unitOfWorkManage"></param>
        /// <param name="sysUserInfoService"></param>
        /// <param name="userRoleService"></param>
        /// <param name="roleService"></param>
        /// <param name="departmentService"></param>
        /// <param name="user"></param>
        /// <param name="mapper"></param>
        /// <param name="logger"></param>
        public UserController(
            IUnitOfWorkManage unitOfWorkManage,
            ISysUserInfoService sysUserInfoService,
            IUserRoleService userRoleService,
            IRoleService roleService,
            IDepartmentService departmentService,
            IUser user,
            IMapper mapper,
            ILogger<UserController> logger)
        {
            _unitOfWorkManage = unitOfWorkManage;
            _sysUserInfoService = sysUserInfoService;
            _userRoleService = userRoleService;
            _roleService = roleService;
            _departmentService = departmentService;
            _user = user;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// 获取全部用户
        /// </summary>
        /// <param name="page"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<PageModel<SysUserInfoDto>>> Get(int page = 1, string key = "")
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrWhiteSpace(key))
            {
                key = "";
            }
            int intPageSize = 50;

            var data = await _sysUserInfoService.QueryPage(a => a.IsDeleted != true && a.Status >= 0 && ((a.LoginName != null && a.LoginName.Contains(key)) || (a.RealName != null && a.RealName.Contains(key))), page, intPageSize, " Id desc ");

            // 这里可以封装到多表查询，此处简单处理
            var allUserRoles = await _userRoleService.Query(it => it.IsDeleted == false);
            var allRoles = await _roleService.Query(d => d.IsDeleted == false);
            var allDepartments = await _departmentService.Query(d => d.IsDeleted == false);

            var sysUserInfos = data.Data;
            foreach (var item in sysUserInfos)
            {
                var currentUserRoles = allUserRoles.Where(it => it.UserId == item.Id)
                    .Select(it => it.RoleId)
                    .ToList();
                item.RIDs = currentUserRoles;
                item.RoleNames = allRoles.Where(it => currentUserRoles.Contains(it.Id))
                    .Select(it => it.Name)
                    .ToList();
                var departmentNameAndIds = GetFullDepartmentName(allDepartments, item.DepartmentId);
                item.DepartmentName = departmentNameAndIds.Item1;
                item.Dids = departmentNameAndIds.Item2;
            }
            data.Data = sysUserInfos;

            return Success(data.AdaptTo<SysUserInfoDto>(_mapper));
        }

        private (string, List<long>) GetFullDepartmentName(List<Department> departments, long departmentId)
        {
            var departmentModel = departments.FirstOrDefault(d => d.Id == departmentId);
            if (departmentModel == null)
            {
                return ("", new List<long>());
            }

            var pids = departmentModel.CodeRelationship?.TrimEnd(',')
                .Split(',')
                .Select(d => d.ObjToLong())
                .ToList();
            pids.Add(departmentModel.Id);
            var pnams = departments.Where(d => pids.Contains(d.Id))
                .ToList()
                .Select(d => d.Name)
                .ToArray();
            var fullName = string.Join("/", pnams);

            return (fullName, pids);
        }

        // GET: api/User/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public string Get(string id)
        {
            _logger.LogError("test wrong");
            return "value";
        }

        /// <summary>
        /// 获取用户详情根据token
        /// 【无权限】
        /// </summary>
        /// <param name="token">令牌</param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<MessageModel<SysUserInfoDto>> GetInfoByToken(string token)
        {
            var data = new MessageModel<SysUserInfoDto>();
            if (!string.IsNullOrEmpty(token))
            {
                var tokenModel = JwtHelper.SerializeJwt(token);
                if (tokenModel != null && tokenModel.Uid > 0)
                {
                    var userinfo = await _sysUserInfoService.QueryById(tokenModel.Uid);
                    if (userinfo != null)
                    {
                        data.Response = _mapper.Map<SysUserInfoDto>(userinfo);
                        data.Success = true;
                        data.Msg = "获取成功";
                    }
                }
            }

            return data;
        }

        /// <summary>
        /// 添加一个用户
        /// </summary>
        /// <param name="sysUserInfo"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MessageModel<string>> Post([FromBody] SysUserInfoDto sysUserInfo)
        {
            var data = new MessageModel<string>();

            sysUserInfo.LoginPWD = MD5Helper.MD5Encrypt32(sysUserInfo.LoginPWD);
            sysUserInfo.Remark = _user.Name;

            var id = await _sysUserInfoService.Add(_mapper.Map<SysUserInfo>(sysUserInfo));
            data.Success = id > 0;
            if (data.Success)
            {
                data.Response = id.ObjToString();
                data.Msg = "添加成功";
            }

            return data;
        }

        /// <summary>
        /// 更新用户与角色
        /// </summary>
        /// <param name="sysUserInfo"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<MessageModel<string>> Put([FromBody] SysUserInfoDto sysUserInfo)
        {
            // 这里使用事务处理
            var data = new MessageModel<string>();

            var oldUser = await _sysUserInfoService.QueryById(sysUserInfo.uID);
            if (oldUser is not { Id: > 0 })
            {
                return Failed<string>("用户不存在或已被删除");
            }

            try
            {
                if (sysUserInfo.LoginPWD != oldUser.LoginPWD)
                {
                    oldUser.CriticalModifyTime = DateTime.Now;
                }

                _mapper.Map(sysUserInfo, oldUser);

                _unitOfWorkManage.BeginTran();
                // 无论 Update Or Add , 先删除当前用户的全部 U_R 关系
                var usreroles = await _userRoleService.Query(it => it.UserId == oldUser.Id);
                if (usreroles.Any())
                {
                    var ids = usreroles.Select(d => d.Id.ToString()).ToArray();
                    var isAllDeleted = await _userRoleService.DeleteByIds(ids);
                    if (!isAllDeleted)
                    {
                        return Failed("服务器更新异常");
                    }
                }

                // 然后再执行添加操作
                if (sysUserInfo.RIDs.Count > 0)
                {
                    var userRolsAdd = new List<UserRole>();
                    sysUserInfo.RIDs.ForEach(rid => { userRolsAdd.Add(new UserRole(oldUser.Id, rid)); });

                    var oldRole = usreroles.Select(s => s.RoleId)
                        .OrderBy(i => i)
                        .ToArray();
                    var newRole = userRolsAdd.Select(s => s.RoleId)
                        .OrderBy(i => i)
                        .ToArray();
                    if (!oldRole.SequenceEqual(newRole))
                    {
                        oldUser.CriticalModifyTime = DateTime.Now;
                    }

                    await _userRoleService.Add(userRolsAdd);
                }

                data.Success = await _sysUserInfoService.Update(oldUser);

                _unitOfWorkManage.CommitTran();

                if (data.Success)
                {
                    data.Msg = "更新成功";
                    data.Response = oldUser.Id.ObjToString();
                }
            }
            catch (Exception e)
            {
                _unitOfWorkManage.RollbackTran();
                _logger.LogError(e, e.Message);
            }

            return data;
        }

        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<MessageModel<string>> Delete(long id)
        {
            var data = new MessageModel<string>();
            if (id > 0)
            {
                var userDetail = await _sysUserInfoService.QueryById(id);
                userDetail.IsDeleted = true;
                data.Success = await _sysUserInfoService.Update(userDetail);
                if (data.Success)
                {
                    data.Msg = "删除成功";
                    data.Response = userDetail?.Id.ObjToString();
                }
            }

            return data;
        }
    }
}
