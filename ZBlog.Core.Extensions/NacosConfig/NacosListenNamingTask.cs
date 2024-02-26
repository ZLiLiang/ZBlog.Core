using Microsoft.Extensions.Hosting;
using Nacos.V2;
using Nacos.V2.Common;
using Nacos.V2.Naming.Dtos;
using Nacos.V2.Naming.Event;
using Newtonsoft.Json;
using ZBlog.Core.Common.Helper;
using ZBlog.Core.Common.Helper.Console;

namespace ZBlog.Core.Extensions.NacosConfig
{
    public class NacosListenNamingTask : BackgroundService
    {
        private readonly INacosNamingService _nacosNamingService;

        /// <summary>
        /// 监听事件
        /// </summary>
        private NamingServiceEventListener eventListener = new NamingServiceEventListener();

        public NacosListenNamingTask(INacosNamingService nacosNamingService)
        {
            _nacosNamingService = nacosNamingService;
        }

        /// <summary>
        /// 订阅服务变化 
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Add listener
            await _nacosNamingService.Subscribe(JsonConfigSettings.NacosServiceName, Constants.DEFAULT_GROUP, eventListener);
            var instance = new Instance
            {
                ServiceName = JsonConfigSettings.NacosServiceName,
                ClusterName = Constants.DEFAULT_CLUSTER_NAME,
                Ip = IpHelper.GetCurrentIp(null),
                Port = JsonConfigSettings.NacosPort,
                Enabled = true,
                Weight = 1000,// 权重 默认1000
                Metadata = JsonConfigSettings.NacosMetadata
            };
            await _nacosNamingService.RegisterInstance(JsonConfigSettings.NacosServiceName, Constants.DEFAULT_GROUP, instance);
            ConsoleHelper.WriteSuccessLine($"Nacos connect: Success!");
        }

        /// <summary>
        /// 程序停止
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            // Remove listener
            await _nacosNamingService.Unsubscribe(JsonConfigSettings.NacosServiceName, Constants.DEFAULT_GROUP, eventListener);
            await _nacosNamingService.DeregisterInstance(JsonConfigSettings.NacosServiceName, Constants.DEFAULT_GROUP, IpHelper.GetCurrentIp(null), JsonConfigSettings.NacosPort);

            await base.StopAsync(cancellationToken);
        }
    }

    /// <summary>
    /// 服务变更事件监听
    /// </summary>
    public class NamingServiceEventListener : IEventListener
    {
        /// <summary>
        /// 监听事件
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        public Task OnEvent(IEvent @event)
        {
            if (@event is InstancesChangeEvent e)
            {
                Console.WriteLine($"==========收到服务变更事件=======》{JsonConvert.SerializeObject(e)}");

                // 配置有变动后 刷新redis配置 刷新 mq配置

                //_redisCachqManager.DisposeRedisConnection();
            }

            return Task.CompletedTask;
        }
    }
}
