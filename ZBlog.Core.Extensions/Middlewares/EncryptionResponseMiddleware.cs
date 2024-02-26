using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using ZBlog.Core.Common.Extensions;
using ZBlog.Core.Common.Helper;

namespace ZBlog.Core.Extensions.Middlewares
{
    /// <summary>
    /// 自定义中间件
    /// 通过配置，对指定接口返回数据进行加密返回
    /// 可过滤文件流
    /// </summary>
    public class EncryptionResponseMiddleware
    {
        private readonly RequestDelegate _next;

        public EncryptionResponseMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // 配置开关，过滤接口
            if (AppSettings.App("Middleware", "EncryptionResponse", "Enabled").ObjToBool())
            {
                var isAllApis = AppSettings.App("Middleware", "EncryptionResponse", "AllApis").ObjToBool();
                var needEnApis = AppSettings.App<string>("Middleware", "EncryptionResponse", "LimitApis");
                var path = context.Request.Path.Value.ToLower();

                if (isAllApis || (path.Length > 5 && needEnApis.Any(d => d.ToLower().Contains(path))))
                {
                    Console.WriteLine($"{isAllApis} -- {path}");
                    var responseCxt = context.Response;
                    var originalBodyStream = responseCxt.Body;

                    // 创建一个新的内存流用于存储加密后的数据
                    using var encryptedBodyStream = new MemoryStream();
                    // 用新的内存流替换 responseCxt.Body
                    responseCxt.Body = encryptedBodyStream;

                    // 执行下一个中间件请求管道
                    await _next(context);

                    // 可以去掉某些流接口
                    if (!context.Response.ContentType.ToLower().Contains("application/json"))
                    {
                        Console.WriteLine($"非json返回格式 {context.Response.ContentType}");
                        context.Response.Body = originalBodyStream;
                        return;
                    }

                    // 读取加密后的数据
                    var encryptedBody = responseCxt.GetResponseBody();

                    if (encryptedBody.IsNotEmptyOrNull())
                    {
                        //可能存在一些错误
                        dynamic jsonObject = JsonConvert.DeserializeObject(encryptedBody);
                        string statusCont = jsonObject.Status;
                        var status = statusCont.ObjToInt();
                        string msg = jsonObject.Msg;
                        string successCont = jsonObject.Success;
                        var success = successCont.ObjToBool();
                        dynamic responseCnt = success ? jsonObject.Response : "";
                        string s = "1";
                        // 这里换成自己的任意加密方式
                        var response = responseCnt.ToString() != "" ? Convert.ToBase64String(Encoding.UTF8.GetBytes(responseCnt.ToString())) : "";
                        string resJson = JsonConvert.SerializeObject(new { response, msg, status, s, success });

                        context.Response.Clear();
                        responseCxt.ContentType = "application/json";

                        var encryptedData = Encoding.UTF8.GetBytes(resJson);
                        responseCxt.ContentLength = encryptedData.Length;
                        await originalBodyStream.WriteAsync(encryptedData, 0, encryptedData.Length);

                        responseCxt.Body = originalBodyStream;
                    }
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

    public static class EncryptionResponseExtension
    {
        /// <summary>
        /// 自定义中间件
        /// 通过配置，对指定接口返回数据进行加密返回
        /// 可过滤文件流
        /// 注意：放到管道最外层
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseEncryptionResponse(this IApplicationBuilder app)
        {
            return app.UseMiddleware<EncryptionResponseMiddleware>();
        }
    }
}
