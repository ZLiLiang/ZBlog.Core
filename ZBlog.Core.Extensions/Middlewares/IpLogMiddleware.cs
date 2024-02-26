using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using ZBlog.Core.Common.Helper;
using ZBlog.Core.Common.LogHelper;

namespace ZBlog.Core.Extensions.Middlewares
{
    /// <summary>
    /// 中间件
	/// 记录IP请求数据
    /// </summary>
    public class IpLogMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IWebHostEnvironment _environment;

        public IpLogMiddleware(RequestDelegate next, IWebHostEnvironment environment)
        {
            _next = next;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (AppSettings.App("Middleware", "IPLog", "Enabled").ObjToBool())
            {
                // 过滤，只有接口
                if (context.Request.Path.Value.Contains("api"))
                {
                    context.Request.EnableBuffering();

                    // 存储请求数据
                    var request = context.Request;

                    var requestInfo = JsonConvert.SerializeObject(new RequestInfo
                    {
                        Ip = GetClientIP(context),
                        Url = request.Path.ObjToString().TrimEnd('/').ToLower(),
                        Datetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        Date = DateTime.Now.ToString("yyyy-MM-dd"),
                        Week = GetWeek(),
                    });

                    if (!string.IsNullOrEmpty(requestInfo))
                    {
                        // 自定义log输出
                        Parallel.For(0, 1, e =>
                        {
                            LogLock.OutLogAOP("RequestIpInfoLog",
                                context.TraceIdentifier,
                                new string[] { requestInfo.GetType().ToString(), requestInfo },
                                false);
                        });

                        request.Body.Position = 0;
                    }

                    await _next(context);
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

        private string GetWeek()
        {
            string week;
            switch (DateTime.Now.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    week = "周一";
                    break;
                case DayOfWeek.Tuesday:
                    week = "周二";
                    break;
                case DayOfWeek.Wednesday:
                    week = "周三";
                    break;
                case DayOfWeek.Thursday:
                    week = "周四";
                    break;
                case DayOfWeek.Friday:
                    week = "周五";
                    break;
                case DayOfWeek.Saturday:
                    week = "周六";
                    break;
                case DayOfWeek.Sunday:
                    week = "周日";
                    break;
                default:
                    week = "N/A";
                    break;
            }

            return week;
        }

        public static string GetClientIP(HttpContext context)
        {
            var ip = context.Request.Headers["X-Forwarded-For"].ObjToString();
            if (string.IsNullOrEmpty(ip))
                ip = context.Connection.RemoteIpAddress.ObjToString();

            return ip;
        }
    }
}
