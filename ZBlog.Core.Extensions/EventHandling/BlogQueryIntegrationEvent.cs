using ZBlog.Core.EventBus.Eventbus;

namespace ZBlog.Core.Extensions.EventHandling
{
    public class BlogQueryIntegrationEvent : IntegrationEvent
    {
        public string BlogId { get; private set; }

        public BlogQueryIntegrationEvent(string blogId)
        {
            BlogId = blogId;
        }
    }
}
