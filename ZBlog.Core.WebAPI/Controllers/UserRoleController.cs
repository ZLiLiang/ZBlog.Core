using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZBlog.Core.Common.GlobalVars;
using ZBlog.Core.IServices;
using ZBlog.Core.Model.ViewModels;
using ZBlog.Core.Model;
using ZBlog.Core.Model.Models;
using ZBlog.Core.Services;

namespace ZBlog.Core.WebAPI.Controllers
{
    /// <summary>
    /// 用户角色关系
    /// </summary>
    [Produces("application/json")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Permissions.Name)]
    public class UserRoleController : ControllerBase
    {
        private readonly ISysUserInfoService _sysUserInfoService;
        private readonly IUserRoleService _userRoleService;
        private readonly IRoleService _roleService;
        private readonly IMapper _mapper;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="sysUserInfoService"></param>
        /// <param name="userRoleService"></param>
        /// <param name="roleService"></param>
        /// <param name="mapper"></param>
        public UserRoleController(
            ISysUserInfoService sysUserInfoService,
            IUserRoleService userRoleService,
            IRoleService roleService,
            IMapper mapper)
        {
            _sysUserInfoService = sysUserInfoService;
            _userRoleService = userRoleService;
            _roleService = roleService;
            _mapper = mapper;
        }

        /// <summary>
        /// 新建用户
        /// </summary>
        /// <param name="loginName"></param>
        /// <param name="loginPwd"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<SysUserInfoDto>> AddUser(string loginName, string loginPwd)
        {
            var userInfo = await _sysUserInfoService.SaveUserInfo(loginName, loginPwd);

            return new MessageModel<SysUserInfoDto>
            {
                Success = true,
                Msg = "添加成功",
                Response = _mapper.Map<SysUserInfoDto>(userInfo)
            };
        }

        /// <summary>
        /// 新建Role
        /// </summary>
        /// <param name="roleName"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<Role>> AddRole(string roleName)
        {
            return new MessageModel<Role>
            {
                Success = true,
                Msg = "添加成功",
                Response = await _roleService.SaveRole(roleName)
            };
        }

        /// <summary>
        /// 新建用户角色关系
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="rid"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<UserRole>> AddUserRole(long uid, long rid)
        {
            return new MessageModel<UserRole>()
            {
                Success = true,
                Msg = "添加成功",
                Response = await _userRoleService.SaveUserRole(uid, rid)
            };
        }
    }
}
