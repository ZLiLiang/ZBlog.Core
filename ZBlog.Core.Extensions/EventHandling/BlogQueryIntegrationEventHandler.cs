using Microsoft.Extensions.Logging;
using ZBlog.Core.Common.Helper.Console;
using ZBlog.Core.EventBus.Eventbus;
using ZBlog.Core.IServices;
using ZBlog.Core.Services;

namespace ZBlog.Core.Extensions.EventHandling
{
    public class BlogQueryIntegrationEventHandler : IIntegrationEventHandler<BlogQueryIntegrationEvent>
    {
        private readonly IBlogArticleService _blogArticleService;
        private readonly ILogger<BlogQueryIntegrationEventHandler> _logger;

        public BlogQueryIntegrationEventHandler(IBlogArticleService blogArticleService, ILogger<BlogQueryIntegrationEventHandler> logger)
        {
            _blogArticleService = blogArticleService;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Handle(BlogQueryIntegrationEvent @event)
        {
            _logger.LogInformation("----- Handling integration event: {IntegrationEventId} at {AppName} - ({@IntegrationEvent})", @event.Id, "Blog.Core", @event);

            ConsoleHelper.WriteSuccessLine($"----- Handling integration event: {@event.Id} at Blog.Core - ({@event})");

            await _blogArticleService.QueryById(@event.BlogId.ToString());
        }
    }
}
