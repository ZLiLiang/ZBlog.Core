using Microsoft.Extensions.DependencyInjection;
using ZBlog.Core.Extensions.HostedService;

namespace ZBlog.Core.Extensions.ServiceExtensions
{
    public static class InitializationHostServiceSetup
    {
        /// <summary>
        /// 应用初始化服务注入
        /// </summary>
        /// <param name="services"></param>
        public static void AddInitializationHostServiceSetup(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddHostedService<SeedDataHostedService>();
            services.AddHostedService<QuartzJobHostedService>();
            services.AddHostedService<ConsulHostedService>();
            services.AddHostedService<EventBusHostedService>();

            // 任务调度 启动服务
            services.AddHostedService<Job1TimedService>();
            services.AddHostedService<Job2TimedService>();
        }
    }
}
