using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZBlog.Core.Common.HttpContextUser;
using ZBlog.Core.IServices.Base;
using ZBlog.Core.Model;
using ZBlog.Core.Model.Models.Tenant;

namespace ZBlog.Core.WebAPI.Controllers.Tenant
{
    /// <summary>
    /// 多租户-多表方案 测试
    /// </summary>
    [Produces("application/json")]
    [Route("api/Tenant/ByTable")]
    [Authorize]
    public class TenantByTableController : BaseApiController
    {
        private readonly IBaseService<MultiBusinessTable> _service;
        private readonly IUser _user;

        public TenantByTableController(IBaseService<MultiBusinessTable> service, IUser user)
        {
            _service = service;
            _user = user;
        }

        /// <summary>
        /// 获取租户下全部业务数据
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<List<MultiBusinessTable>>> GetAll()
        {
            //查询
            // var data = await _service.Query();

            //关联查询
            var data = await _service.Db.Queryable<MultiBusinessTable>()
                .Includes(s => s.SubTables)
                .ToListAsync();

            return Success(data);
        }

        /// <summary>
        /// 新增数据
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MessageModel> Post(MultiBusinessTable data)
        {
            await _service.Db.Insertable(data).ExecuteReturnSnowflakeIdAsync();

            return Success();
        }
    }
}
