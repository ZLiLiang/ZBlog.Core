using Microsoft.AspNetCore.Http;
using ZBlog.Core.Extensions.Authorization.Helpers;

namespace ZBlog.Core.Extensions.Middlewares
{
    /// <summary>
    /// 中间件
    /// 原做为自定义授权中间件
    /// 先做检查 header token的使用
    /// </summary>
    public class JwtTokenAuthMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtTokenAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        private void PreProceed(HttpContext next)
        {
            //Console.WriteLine($"{DateTime.Now} middleware invoke preproceed");
            //...
        }
        private void PostProceed(HttpContext next)
        {
            //Console.WriteLine($"{DateTime.Now} middleware invoke postproceed");
            //....
        }

        public Task Invoke(HttpContext httpContext)
        {
            PreProceed(httpContext);

            //检测是否包含'Authorization'请求头
            if (!httpContext.Request.Headers.ContainsKey("Authorization"))
            {
                PostProceed(httpContext);

                return _next(httpContext);
            }

            var tokenHeader = httpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            try
            {
                if (tokenHeader.Length>=128)
                {
                    TokenModelJwt tm = JwtHelper.SerializeJwt(tokenHeader);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now} middleware wrong:{ex.Message}");
            }

            PostProceed(httpContext);


            return _next(httpContext);
        }
    }
}
