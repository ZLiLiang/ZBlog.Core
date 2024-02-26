using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ZBlog.Core.Common.Helper;
using ZBlog.Core.EventBus.EventBusKafka;

namespace ZBlog.Core.Extensions.ServiceExtensions
{
    /// <summary>
    /// 注入Kafka相关配置
    /// </summary>
    public static class KafkaSetup
    {
        public static void AddKafkaSetup(this IServiceCollection services, IConfiguration configuration)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (AppSettings.App(new string[] { "Kafka", "Enabled" }).ObjToBool())
            {
                services.Configure<KafkaOptions>(configuration.GetSection("kafka"));
                services.AddSingleton<IKafkaConnectionPool, KafkaConnectionPool>();
            }
        }
    }
}
