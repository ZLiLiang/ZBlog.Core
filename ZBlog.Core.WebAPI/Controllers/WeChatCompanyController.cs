using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZBlog.Core.Common.GlobalVars;
using ZBlog.Core.IServices;
using ZBlog.Core.Model.Models;
using ZBlog.Core.Model;
using ZBlog.Core.Services;

namespace ZBlog.Core.WebAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Permissions.Name)]
    public class WeChatCompanyController : ControllerBase
    {
        private readonly IWeChatCompanyService _weChatCompanyService;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="weChatCompanyService"></param>
        public WeChatCompanyController(IWeChatCompanyService weChatCompanyService)
        {
            _weChatCompanyService = weChatCompanyService;
        }

        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="pagination">分页条件</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<PageModel<WeChatCompany>>> Get([FromQuery] PaginationModel pagination)
        {
            var data = await _weChatCompanyService.QueryPage(pagination);

            return new MessageModel<PageModel<WeChatCompany>>
            {
                Success = true,
                Response = data
            };
        }

        /// <summary>
        /// 获取(id)
        /// </summary>
        /// <param name="id">主键ID</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<MessageModel<WeChatCompany>> Get(string id)
        {
            var data = await _weChatCompanyService.QueryById(id);

            return new MessageModel<WeChatCompany>
            {
                Success = true,
                Response = data
            };
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<MessageModel<string>> Post([FromBody] WeChatCompany obj)
        {
            await _weChatCompanyService.Add(obj);
            return new MessageModel<string> { Success = true };
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        public async Task<MessageModel<string>> Put([FromBody] WeChatCompany obj)
        {
            await _weChatCompanyService.Update(obj);
            return new MessageModel<string> { Success = true };
        }

        /// <summary>
        /// 删除
        /// </summary> 
        /// <returns></returns> 
        [HttpDelete]
        public async Task<MessageModel<string>> Delete(string id)
        {
            await _weChatCompanyService.DeleteById(id);
            return new MessageModel<string> { Success = true };
        }

        /// <summary>
        /// 批量删除
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        public async Task<MessageModel<string>> BatchDelete(string ids)
        {
            var i = ids.Split(",");
            await _weChatCompanyService.DeleteByIds(i);
            return new MessageModel<string> { Success = true };
        }
    }
}
