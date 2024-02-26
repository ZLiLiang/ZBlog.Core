using System.Diagnostics;
using System.Text;
using System.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ZBlog.Core.Common.Helper;
using ZBlog.Core.Common.HttpContextUser;
using ZBlog.Core.Common.LogHelper;

namespace ZBlog.Core.Extensions.Middlewares
{
    /// <summary>
    /// 中间件
	/// 记录用户方访问数据
    /// </summary>
    public class RecordAccessLogsMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly IUser _user;
        private readonly ILogger<RecordAccessLogsMiddleware> _logger;
        private readonly IWebHostEnvironment _environment;
        private Stopwatch _stopwatch;

        public RecordAccessLogsMiddleware(RequestDelegate next,
            IUser user,
            ILogger<RecordAccessLogsMiddleware> logger,
            IWebHostEnvironment environment)
        {
            _next = next;
            _user = user;
            _logger = logger;
            _environment = environment;
            _stopwatch = new Stopwatch();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (AppSettings.App("Middleware", "RecordAccessLogs", "Enabled").ObjToBool())
            {
                var api = context.Request.Path.ObjToString().TrimEnd('/').ToLower();
                var ignoreApis = AppSettings.App("Middleware", "RecordAccessLogs", "IgnoreApis");

                // 过滤，只有接口
                if (api.Contains("api") && !ignoreApis.Contains(api))
                {
                    _stopwatch.Restart();
                    HttpRequest request = context.Request;

                    var userAccessModel = new UserAccessModel
                    {
                        API = api,
                        User = _user.Name,
                        IP = IpLogMiddleware.GetClientIP(context),
                        BeginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        RequestMethod = request.Method,
                        Agent = request.Headers["User-Agent"].ObjToString()
                    };

                    // 获取请求body内容
                    if (request.Method.ToLower().Equals("post") || request.Method.ToLower().Equals("put"))
                    {
                        // 启用倒带功能，就可以让 Request.Body 可以再次读取
                        request.EnableBuffering();

                        Stream stream = request.Body;
                        byte[] buffer = new byte[request.ContentLength.Value];
                        stream.Read(buffer, 0, buffer.Length);
                        userAccessModel.RequestData = Encoding.UTF8.GetString(buffer);

                        request.Body.Position = 0;
                    }
                    else if (request.Method.ToLower().Equals("get") || request.Method.ToLower().Equals("delete"))
                    {
                        userAccessModel.RequestData = HttpUtility.UrlDecode(request.QueryString.ObjToString(), Encoding.UTF8);
                    }

                    await _next(context);

                    // 响应完成记录时间和存入日志
                    context.Response.OnCompleted(() =>
                    {
                        _stopwatch.Stop();

                        userAccessModel.OPTime = _stopwatch.ElapsedMilliseconds + "ms";

                        // 自定义log输出
                        var requestInfo = JsonConvert.SerializeObject(userAccessModel);
                        Parallel.For(0, 1, e =>
                        {
                            LogLock.OutLogAOP("RecordAccessLogs",
                                context.TraceIdentifier,
                                new string[] { userAccessModel.GetType().ToString(), requestInfo },
                                false);
                        });

                        return Task.CompletedTask;
                    });
                }
                else
                {
                    await _next(context);
                }
            }
            else
            {
                await _next(context);
            }
        }
    }

    public class UserAccessModel
    {
        public string User { get; set; }
        public string IP { get; set; }
        public string API { get; set; }
        public string BeginTime { get; set; }
        public string OPTime { get; set; }
        public string RequestMethod { get; set; }
        public string RequestData { get; set; }
        public string Agent { get; set; }
    }
}
