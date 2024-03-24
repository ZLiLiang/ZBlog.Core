using Microsoft.AspNetCore.Mvc;
using ZBlog.Core.Model;

namespace ZBlog.Core.WebAPI.Controllers
{
    public class BaseApiController : Controller
    {
        [NonAction]
        public MessageModel<T> Success<T>(T data, string msg = "成功")
        {
            return new MessageModel<T>
            {
                Success = true,
                Msg = msg,
                Response = data
            };
        }

        [NonAction]
        public MessageModel Success(string msg = "成功")
        {
            return new MessageModel
            {
                Success = true,
                Msg = msg,
                Response = null
            };
        }

        [NonAction]
        public MessageModel<string> Failed(string msg = "失败", int status = 500)
        {
            return new MessageModel<string>
            {
                Success = false,
                Status = status,
                Msg = msg,
                Response = null
            };
        }

        [NonAction]
        public MessageModel<T> Failed<T>(string msg = "失败", int status = 500)
        {
            return new MessageModel<T>
            {
                Success = false,
                Status = status,
                Msg = msg,
                Response = default
            };
        }

        [NonAction]
        public MessageModel<PageModel<T>> SuccessPage<T>(int page,
                                                         int dataCount,
                                                         int pageSize,
                                                         List<T> data,
                                                         int pageCount,
                                                         string msg = "获取成功")
        {
            return new MessageModel<PageModel<T>>
            {
                Success = true,
                Msg = msg,
                Response = new PageModel<T>(page, dataCount, pageSize, data)
            };
        }

        [NonAction]
        public MessageModel<PageModel<T>> SuccessPage<T>(PageModel<T> pageModel, string msg = "获取成功")
        {
            return new MessageModel<PageModel<T>>
            {
                Success = true,
                Msg = msg,
                Response = pageModel
            };
        }
    }
}
