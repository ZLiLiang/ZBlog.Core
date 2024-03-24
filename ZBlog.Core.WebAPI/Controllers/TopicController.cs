using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZBlog.Core.IServices;
using ZBlog.Core.Model.Models;
using ZBlog.Core.Model;

namespace ZBlog.Core.WebAPI.Controllers
{
    /// <summary>
    /// 类别管理【无权限】
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class TopicController : ControllerBase
    {
        private readonly ITopicService _topicService;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="topicService"></param>
        public TopicController(ITopicService topicService)
        {
            _topicService = topicService;
        }

        /// <summary>
        /// 获取Tibug所有分类
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<List<Topic>>> Get()
        {
            var data = new MessageModel<List<Topic>>
            {
                Response = await _topicService.GetTopics()
            };

            if (data.Response != null)
            {
                data.Success = true;
                data.Msg = "";
            }

            return data;
        }

        // GET: api/Topic/5
        [HttpGet("{id}")]
        public string Get(long id)
        {
            return "value";
        }

        // POST: api/Topic
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT: api/Topic/5
        [HttpPut("{id}")]
        public void Put(long id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(long id)
        {
        }
    }
}
