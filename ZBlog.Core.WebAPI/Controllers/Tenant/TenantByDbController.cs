using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZBlog.Core.Common.HttpContextUser;
using ZBlog.Core.IServices.Base;
using ZBlog.Core.Model;
using ZBlog.Core.Model.Models.Tenant;

namespace ZBlog.Core.WebAPI.Controllers.Tenant
{
    /// <summary>
    /// 多租户-多库方案 测试
    /// </summary>
    [Produces("application/json")]
    [Route("api/Tenant/ByDb")]
    [Authorize]
    public class TenantByDbController : BaseApiController
    {
        private readonly IBaseService<SubLibraryBusinessTable> _service;
        private readonly IUser _user;

        public TenantByDbController(IBaseService<SubLibraryBusinessTable> service, IUser user)
        {
            _service = service;
            _user = user;
        }

        /// <summary>
        /// 获取租户下全部业务数据
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<List<SubLibraryBusinessTable>>> GetAll()
        {
            var data = await _service.Query();

            return Success(data);
        }

        /// <summary>
        /// 新增数据
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MessageModel> Post(SubLibraryBusinessTable data)
        {
            await _service.Db.Insertable(data)
                .ExecuteReturnSnowflakeIdAsync();

            return Success();
        }
    }
}
