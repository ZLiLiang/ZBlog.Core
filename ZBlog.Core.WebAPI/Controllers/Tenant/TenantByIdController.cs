using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZBlog.Core.Common.HttpContextUser;
using ZBlog.Core.IServices.Base;
using ZBlog.Core.Model;
using ZBlog.Core.Model.Models.Tenant;

namespace ZBlog.Core.WebAPI.Controllers.Tenant
{
    /// <summary>
    /// 多租户-Id方案 测试
    /// </summary>
    [Produces("application/json")]
    [Route("api/Tenant/ById")]
    [Authorize]
    public class TenantByIdController : BaseApiController
    {
        private readonly IBaseService<BusinessTable> _service;
        private readonly IUser _user;

        public TenantByIdController(IBaseService<BusinessTable> service, IUser user)
        {
            _service = service;
            _user = user;
        }

        /// <summary>
        /// 获取租户下全部业务数据
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<List<BusinessTable>>> GetAll()
        {
            var data = await _service.Query();

            return Success(data);
        }

        /// <summary>
        /// 新增业务数据
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<MessageModel> Post([FromBody] BusinessTable data)
        {
            await _service.Db.Insertable(data).ExecuteReturnSnowflakeIdAsync();
            return Success();
        }
    }
}
