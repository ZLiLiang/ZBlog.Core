using Mapster;
using MapsterMapper;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using ZBlog.Core.Extensions.Mapster;

namespace ZBlog.Core.Extensions.ServiceExtensions
{
    /// <summary>
    /// Mapster 启动服务
    /// </summary>
    public static class MapsterSetup
    {
        public static void AddMapsterSetup(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            var typeAdapterConfig = MapsterConfig.RegisterMappings();
            services.AddSingleton(typeAdapterConfig);
            services.AddScoped<IMapper, ServiceMapper>();
        }
    }
}
