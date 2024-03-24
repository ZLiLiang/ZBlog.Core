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
    public class WeChatSubController : ControllerBase
    {
        private readonly IWeChatSubService _weChatSubService;

        public WeChatSubController(IWeChatSubService weChatSubService)
        {
            _weChatSubService = weChatSubService;
        }

        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="pagination">分页条件</param> 
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<PageModel<WeChatSub>>> Get([FromQuery] PaginationModel pagination)
        {
            var data = await _weChatSubService.QueryPage(pagination);
            return new MessageModel<PageModel<WeChatSub>>
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
        public async Task<MessageModel<WeChatSub>> Get(string id)
        {
            var data = await _weChatSubService.QueryById(id);
            return new MessageModel<WeChatSub>
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
        public async Task<MessageModel<string>> Post([FromBody] WeChatSub obj)
        {
            await _weChatSubService.Add(obj);
            return new MessageModel<string> { Success = true };
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        public async Task<MessageModel<string>> Put([FromBody] WeChatSub obj)
        {
            await _weChatSubService.Update(obj);
            return new MessageModel<string> { Success = true };
        }

        /// <summary>
        /// 删除
        /// </summary> 
        /// <returns></returns> 
        [HttpDelete]
        public async Task<MessageModel<string>> Delete(string id)
        {
            await _weChatSubService.DeleteById(id);
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
            await _weChatSubService.DeleteByIds(i);
            return new MessageModel<string> { Success = true };
        }
    }
}
