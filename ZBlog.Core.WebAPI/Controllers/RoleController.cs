using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZBlog.Core.Common.GlobalVars;
using ZBlog.Core.Common.Helper;
using ZBlog.Core.Common.HttpContextUser;
using ZBlog.Core.IServices;
using ZBlog.Core.Model;
using ZBlog.Core.Model.Models;
using ZBlog.Core.Services;

namespace ZBlog.Core.WebAPI.Controllers
{
    /// <summary>
    /// 角色管理
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Permissions.Name)]
    public class RoleController : BaseApiController
    {
        private readonly IRoleService _roleService;
        private readonly IUser _user;

        public RoleController(IRoleService roleService, IUser user)
        {
            _roleService = roleService;
            _user = user;
        }

        /// <summary>
        ///  获取全部角色
        /// </summary>
        /// <param name="page"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<PageModel<Role>>> Get(int page = 1, string key = "")
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrWhiteSpace(key))
            {
                key = "";
            }

            int intPageSize = 50;

            var data = await _roleService.QueryPage(a => a.IsDeleted != true && (a.Name != null && a.Name.Contains(key)), page, intPageSize, " Id desc ");

            return Success(data, "获取成功");
        }

        // GET: api/User/5
        [HttpGet("{id}")]
        public string Get(string id)
        {
            return "value";
        }

        /// <summary>
        /// 添加角色
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MessageModel<string>> Post([FromBody] Role role)
        {
            role.CreateId = _user.ID;
            role.CreateBy = _user.Name;
            var id = (await _roleService.Add(role));

            return id > 0 ? Success(id.ObjToString(), "添加成功") : Failed("添加失败");
        }

        /// <summary>
        /// 更新角色
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<MessageModel<string>> Put([FromBody] Role role)
        {
            if (role == null || role.Id <= 0)
                return Failed("缺少参数");

            return await _roleService.Update(role) ? Success(role?.Id.ObjToString(), "更新成功") : Failed("更新失败");
        }

        /// <summary>
        /// 删除角色
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<MessageModel<string>> Delete(long id)
        {
            var data = new MessageModel<string>();
            if (id <= 0)
                return Failed();
            var userDetail = await _roleService.QueryById(id);
            if (userDetail == null)
                return Success<string>(null, "角色不存在");
            userDetail.IsDeleted = true;

            return await _roleService.Update(userDetail) ? Success(userDetail?.Id.ObjToString(), "删除成功") : Failed();
        }
    }
}
