using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Builder;
using Serilog;
using ZBlog.Core.Common.Helper;

namespace ZBlog.Core.Extensions.Middlewares
{
    /// <summary>
    /// ip 限流
    /// </summary>
    public static class IpLimitMiddleware
    {
        public static void UseIpLimitMiddle(this IApplicationBuilder app)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));

            try
            {
                if (AppSettings.App("Middleware", "IpRateLimit", "Enabled").ObjToBool())
                    app.UseIpRateLimiting();
            }
            catch (Exception ex)
            {
                Log.Error($"Error occured limiting ip rate.\n{ex.Message}");
                throw;
            }
        }
    }
}
