using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ZBlog.Core.Common.Helper;
using ZBlog.Core.EventBus.Eventbus;
using ZBlog.Core.Extensions.EventHandling;

namespace ZBlog.Core.Extensions.HostedService
{
    public class EventBusHostedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EventBusHostedService> _logger;

        public EventBusHostedService(IServiceProvider serviceProvider, ILogger<EventBusHostedService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Start EventBus Service!");
            await DoWork();
        }

        private Task DoWork()
        {
            if (AppSettings.App(new string[] { "EventBus", "Enabled" }).ObjToBool())
            {
                var eventBus = _serviceProvider.GetRequiredService<IEventBus>();
                eventBus.Subscribe<BlogQueryIntegrationEvent, BlogQueryIntegrationEventHandler>();
            }
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stop EventBus Service!");
            return Task.CompletedTask;
        }
    }
}
