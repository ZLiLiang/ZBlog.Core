using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZBlog.Core.IServices;
using ZBlog.Core.Model.Models;
using ZBlog.Core.Model;

namespace ZBlog.Core.WebAPI.Controllers.Tenant
{
    /// <summary>
    /// 租户管理
    /// </summary>
    [Produces("application/json")]
    [Route("api/TenantManager")]
    [Authorize]
    public class TenantManagerController : BaseApiController
    {
        private readonly ITenantService _service;

        public TenantManagerController(ITenantService service)
        {
            _service = service;
        }

        /// <summary>
        /// 获取全部租户
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<List<SysTenant>>> GetAll()
        {
            var data = await _service.Query();

            return Success(data);
        }

        /// <summary>
        /// 获取租户信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<MessageModel<SysTenant>> GetInfo(long id)
        {
            var data = await _service.QueryById(id);

            return Success(data);
        }

        /// <summary>
        /// 新增租户信息 <br/>
        /// 此处只做演示，具体要以实际业务为准
        /// </summary>
        /// <param name="tenant"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MessageModel> Post(SysTenant tenant)
        {
            await _service.SaveTenant(tenant);

            return Success();
        }

        /// <summary>
        /// 修改租户信息 <br/>
        /// 此处只做演示，具体要以实际业务为准
        /// </summary>
        /// <param name="tenant"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<MessageModel> Put(SysTenant tenant)
        {
            await _service.SaveTenant(tenant);

            return Success();
        }

        /// <summary>
        /// 删除租户 <br/>
        /// 此处只做演示，具体要以实际业务为准
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<MessageModel> Delete(long id)
        {
            //是否删除租户库?
            //要根据实际情况而定
            //例如直接删除租户库、备份租户库到xx
            await _service.DeleteById(id);

            return Success();
        }
    }
}
