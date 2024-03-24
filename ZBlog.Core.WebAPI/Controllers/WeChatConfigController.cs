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
    public class WeChatConfigController : ControllerBase
    {
        private readonly IWeChatConfigService _weChatConfigService;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="weChatConfigService"></param>
        public WeChatConfigController(IWeChatConfigService weChatConfigService)
        {
            _weChatConfigService = weChatConfigService;
        }

        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="pagination">分页条件</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<PageModel<WeChatConfig>>> Get([FromQuery] PaginationModel pagination)
        {
            var data = await _weChatConfigService.QueryPage(pagination);

            return new MessageModel<PageModel<WeChatConfig>>
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
        public async Task<MessageModel<WeChatConfig>> Get(string id)
        {
            var data = await _weChatConfigService.QueryById(id);

            return new MessageModel<WeChatConfig>
            {
                Success = true,
                Response = data
            };
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MessageModel<string>> Post([FromBody] WeChatConfig obj)
        {
            await _weChatConfigService.Add(obj);
            return new MessageModel<string> { Success = true };
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        public async Task<MessageModel<string>> Put([FromBody] WeChatConfig obj)
        {
            await _weChatConfigService.Update(obj);
            return new MessageModel<string> { Success = true };
        }

        /// <summary>
        /// 删除
        /// </summary> 
        /// <returns></returns> 
        [HttpDelete]
        public async Task<MessageModel<string>> Delete(string id)
        {
            await _weChatConfigService.DeleteById(id);
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
            await _weChatConfigService.DeleteByIds(i);
            return new MessageModel<string> { Success = true };
        }
    }
}
