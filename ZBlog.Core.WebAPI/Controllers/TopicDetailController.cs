using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZBlog.Core.Common.GlobalVars;
using ZBlog.Core.IServices;
using ZBlog.Core.Model.Models;
using ZBlog.Core.Model;
using ZBlog.Core.Common.Helper;
using ZBlog.Core.Services;

namespace ZBlog.Core.WebAPI.Controllers
{
    /// <summary>
    /// Tibug 管理
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Permissions.Name)]
    public class TopicDetailController : ControllerBase
    {
        private readonly ITopicService _topicService;
        private readonly ITopicDetailService _topicDetailService;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="topicService"></param>
        /// <param name="topicDetailService"></param>
        public TopicDetailController(ITopicService topicService, ITopicDetailService topicDetailService)
        {
            _topicService = topicService;
            _topicDetailService = topicDetailService;
        }

        /// <summary>
        /// 获取Bug数据列表（带分页）
        /// 【无权限】
        /// </summary>
        /// <param name="page">页数</param>
        /// <param name="tname">专题类型</param>
        /// <param name="key">关键字</param>
        /// <param name="intPageSize"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<MessageModel<PageModel<TopicDetail>>> Get(
            int page = 1,
            string tname = "",
            string key = "",
            int intPageSize = 12)
        {
            long tid = 0;

            if (string.IsNullOrEmpty(key) || string.IsNullOrWhiteSpace(key))
            {
                key = "";
            }
            if (string.IsNullOrEmpty(tname) || string.IsNullOrWhiteSpace(tname))
            {
                tname = "";
            }
            tname = UnicodeHelper.UnicodeToString(tname);

            if (!string.IsNullOrEmpty(tname))
            {
                tid = ((await _topicService.Query(ts => ts.Name == tname)).FirstOrDefault()?.Id).ObjToLong();
            }

            //烂代码
            var data = await _topicDetailService.QueryPage(a => !a.IsDelete && a.SectendDetail == "tbug" && ((tid == 0 && true) || (tid > 0 && a.TopicId == tid)) && ((a.Name != null && a.Name.Contains(key)) || (a.Detail != null && a.Detail.Contains(key))), page, intPageSize, " Id desc ");

            return new MessageModel<PageModel<TopicDetail>>
            {
                Msg = "获取成功",
                Success = data.DataCount >= 0,
                Response = data
            };
        }

        /// <summary>
        /// 获取详情【无权限】
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<MessageModel<TopicDetail>> Get(long id)
        {
            var data = new MessageModel<TopicDetail>();
            var response = id > 0 ? await _topicDetailService.QueryById(id) : new TopicDetail();
            data.Response = (response?.IsDelete).ObjToBool() ? new TopicDetail() : response;

            if (data.Response != null)
            {
                data.Success = true;
                data.Msg = "";
            }

            return data;
        }

        /// <summary>
        /// 添加一个 BUG 【无权限】
        /// </summary>
        /// <param name="topicDetail"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<MessageModel<string>> Post([FromBody] TopicDetail topicDetail)
        {
            var data = new MessageModel<string>();

            topicDetail.CreateTime = DateTime.Now;
            topicDetail.Read = 0;
            topicDetail.Commend = 0;
            topicDetail.Good = 0;
            topicDetail.Top = 0;

            var id = await _topicDetailService.Add(topicDetail);
            data.Success = id > 0;
            if (data.Success)
            {
                data.Response = id.ObjToString();
                data.Msg = "添加成功";
            }

            return data;
        }

        /// <summary>
        /// 更新 bug
        /// </summary>
        /// <param name="topicDetail"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<MessageModel<string>> Update([FromBody] TopicDetail topicDetail)
        {
            var data = new MessageModel<string>();
            if (topicDetail != null && topicDetail.Id > 0)
            {
                data.Success = await _topicDetailService.Update(topicDetail);
                if (data.Success)
                {
                    data.Msg = "更新成功";
                    data.Response = topicDetail?.Id.ObjToString();
                }
            }

            return data;
        }

        /// <summary>
        /// 删除 bug
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<MessageModel<string>> Delete(long id)
        {
            var data = new MessageModel<string>();
            if (id > 0)
            {
                var topicDetail = await _topicDetailService.QueryById(id);
                topicDetail.IsDelete = true;
                data.Success = await _topicDetailService.Update(topicDetail);

                if (data.Success)
                {
                    data.Msg = "删除成功";
                    data.Response = topicDetail?.Id.ObjToString();
                }
            }

            return data;
        }
    }
}
