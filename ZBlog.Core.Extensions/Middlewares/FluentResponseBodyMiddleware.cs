using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
using ZBlog.Core.Common.Https;

namespace ZBlog.Core.Extensions.Middlewares
{
    public static class FluentResponseBodyMiddleware
    {
        public static IApplicationBuilder UseResponseBodyRead(this IApplicationBuilder app)
        {
            return app.Use(async (context, next) =>
            {
                await using var swapStream = new FluentHttpResponseStream(context.Features.Get<IHttpBodyControlFeature>(), context.Features.Get<IHttpResponseBodyFeature>());
                context.Response.Body = swapStream;
                await next(context);
                context.Response.Body.Seek(0, SeekOrigin.Begin);
            });
        }
    }
}
