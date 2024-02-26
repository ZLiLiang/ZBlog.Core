using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Spi;
using ZBlog.Core.Tasks.QuartzNet;

namespace ZBlog.Core.Extensions.ServiceExtensions
{
    /// <summary>
    /// 任务调度 启动服务
    /// </summary>
    public static class JobSetup
    {
        public static void AddJobSetup(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddSingleton<IJobFactory, JobFactory>();
            services.AddSingleton<ISchedulerCenter, SchedulerCenterServer>();

            //任务注入
            var baseType = typeof(IJob);
            var path = AppDomain.CurrentDomain.RelativeSearchPath ?? AppDomain.CurrentDomain.BaseDirectory;
            var referencedAssemblies = Directory.GetFiles(path, "ZBlog.Core.Tasks.dll")
                .Select(Assembly.LoadFrom)
                .ToArray();
            var types = referencedAssemblies.SelectMany(assembly => assembly.DefinedTypes)
                .Select(type => type.AsType())
                .Where(type => type != baseType && baseType.IsAssignableFrom(type));
            var implementTypes = types.Where(x => x.IsClass);
            foreach (var implementType in implementTypes)
            {
                services.AddTransient(implementType);
            }
        }
    }
}
