using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using ZBlog.Core.Common;
using ZBlog.Core.Common.Caches;
using ZBlog.Core.Common.Helper;
using ZBlog.Core.Common.Option;
using ZBlog.Core.Extensions.Redis;

namespace ZBlog.Core.Extensions.ServiceExtensions
{
    public static class CacheSetup
    {
        /// <summary>
        /// 统一注册缓存
        /// </summary>
        /// <param name="services"></param>
        public static void AddCacheSetup(this IServiceCollection services)
        {
            var cacheOptions = App.GetOptions<RedisOptions>();
            if (cacheOptions.Enable)
            {
                // 配置启动Redis服务，虽然可能影响项目启动速度，但是不能在运行的时候报错，所以是合理的
                services.AddScoped<IConnectionMultiplexer>(sp =>
                {
                    //获取连接字符串
                    var configuration = ConfigurationOptions.Parse(cacheOptions.ConnectionString, true);
                    configuration.ResolveDns = true;

                    return ConnectionMultiplexer.Connect(configuration);
                });
                services.AddSingleton<ConnectionMultiplexer>(sp => sp.GetService<IConnectionMultiplexer>() as ConnectionMultiplexer);

                //使用Redis
                services.AddStackExchangeRedisCache(options =>
                {
                    options.ConnectionMultiplexerFactory = () => Task.FromResult(App.GetService<IConnectionMultiplexer>(false));
                    if (!cacheOptions.InstanceName.IsNullOrEmpty())
                        options.InstanceName = cacheOptions.InstanceName;
                });

                services.AddTransient<IRedisBasketRepository, RedisBasketRepository>();
            }
            else
            {
                //使用内存
                services.AddMemoryCache();
                services.AddDistributedMemoryCache();
            }

            services.AddSingleton<ICaching, Caching>();
        }
    }
}
