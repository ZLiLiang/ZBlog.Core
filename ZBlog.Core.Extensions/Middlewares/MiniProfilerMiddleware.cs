using Microsoft.AspNetCore.Builder;
using Serilog;
using ZBlog.Core.Common.Helper;

namespace ZBlog.Core.Extensions.Middlewares
{
    /// <summary>
    /// MiniProfiler性能分析
    /// </summary>
    public static class MiniProfilerMiddleware
    {
        public static void UseMiniProfilerMiddleware(this IApplicationBuilder app)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));

            try
            {
                if (AppSettings.App("Startup", "MiniProfiler", "Enabled").ObjToBool())
                    app.UseMiniProfiler();// 性能分析
            }
            catch (Exception ex)
            {
                Log.Error($"An error was reported when starting the MiniProfilerMildd.\n{ex.Message}");
                throw;
            }
        }
    }
}
