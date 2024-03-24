
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Profiling;
using ZBlog.Core.Common.Helper;
using ZBlog.Core.Common.Hubs;
using ZBlog.Core.Common.LogHelper;
using ZBlog.Core.Model;

namespace ZBlog.Core.WebAPI.Filter
{
    /// <summary>
    /// 全局异常错误日志
    /// </summary>
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly IWebHostEnvironment environment;
        private readonly ILogger<GlobalExceptionFilter> logger;
        private readonly IHubContext<ChatHub> hubContext;

        public GlobalExceptionFilter(
            IWebHostEnvironment environment,
            ILogger<GlobalExceptionFilter> logger,
            IHubContext<ChatHub> hubContext)
        {
            this.environment = environment;
            this.logger = logger;
            this.hubContext = hubContext;
        }

        public void OnException(ExceptionContext context)
        {
            var json = new MessageModel<string>
            {
                Msg = context.Exception.Message,//错误信息
                Status = 500//500异常 
            };
            var errorAudit = "Unable to resolve service for";
            if (!string.IsNullOrEmpty(json.Msg) && json.Msg.Contains(errorAudit))
            {
                json.Msg = json.Msg.Replace(errorAudit, $"（若新添加服务，需要重新编译项目）{errorAudit}");
            }

            if (environment.EnvironmentName.ObjToString().Equals("Development"))
            {
                json.MsgDev = context.Exception.StackTrace;//堆栈信息
            }
            var res = new ContentResult
            {
                Content = JsonHelper.GetJson<MessageModel<string>>(json)
            };

            context.Result = res;

            MiniProfiler.Current.CustomTiming("Errors：", json.Msg);

            //进行错误日志记录
            logger.LogError(json.Msg + WriteLog(json.Msg, context.Exception));
            if (AppSettings.App(new string[] { "Middleware", "SignalRSendLog", "Enabled" }).ObjToBool())
            {
                hubContext.Clients.All.SendAsync("ReceiveUpdate", LogLock.GetLogData()).Wait();
            }
        }

        /// <summary>
        /// 自定义返回格式
        /// </summary>
        /// <param name="throwMsg"></param>
        /// <param name="ex"></param>
        /// <returns></returns>
        public string WriteLog(string throwMsg, Exception ex)
        {
            return string.Format("\r\n【自定义错误】：{0} \r\n【异常类型】：{1} \r\n【异常信息】：{2} \r\n【堆栈调用】：{3}", new object[] { throwMsg,
                ex.GetType().Name, ex.Message, ex.StackTrace });
        }
    }

    public class InternalServerErrorObjectResult : ObjectResult
    {
        public InternalServerErrorObjectResult(object value) : base(value)
        {
            StatusCode = StatusCodes.Status500InternalServerError;
        }
    }

    /// <summary>
    /// 返回错误信息
    /// </summary>
    public class JsonErrorResponse
    {
        /// <summary>
        /// 生产环境的消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 开发环境的消息
        /// </summary>
        public string DevelopmentMessage { get; set; }
    }
}
