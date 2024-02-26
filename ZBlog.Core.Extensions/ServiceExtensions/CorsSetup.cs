using Microsoft.Extensions.DependencyInjection;
using ZBlog.Core.Common.Helper;

namespace ZBlog.Core.Extensions.ServiceExtensions
{
    /// <summary>
    /// Cors 启动服务
    /// </summary>
    public static class CorsSetup
    {
        public static void AddCorsSetup(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddCors(c =>
            {
                if (!AppSettings.App(new string[] { "Startup", "Cors", "EnableAllIPs" }).ObjToBool())
                {
                    c.AddPolicy(AppSettings.App(new string[] { "Startup", "Cors", "PolicyName" }), policy =>
                    {
                        policy.WithOrigins(AppSettings.App(new string[] { "Startup", "Cors", "IPs" }).Split(','))
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    });
                }
                else
                {
                    //允许任意跨域请求
                    c.AddPolicy(AppSettings.App(new string[] { "Startup", "Cors", "PolicyName" }), policy =>
                    {
                        policy.SetIsOriginAllowed(host => true)
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials();
                    });
                }
            });
        }
    }
}
