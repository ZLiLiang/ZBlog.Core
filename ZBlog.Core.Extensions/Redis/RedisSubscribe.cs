using InitQ.Abstractions;
using InitQ.Attributes;
using ZBlog.Core.Common.GlobalVars;
using ZBlog.Core.IServices;

namespace ZBlog.Core.Extensions.Redis
{
    public class RedisSubscribe : IRedisSubscribe
    {
        private readonly IBlogArticleService _blogArticleService;

        public RedisSubscribe(IBlogArticleService blogArticleService)
        {
            _blogArticleService = blogArticleService;
        }

        [Subscribe(RedisMqKey.Loging)]
        private async Task SubRedisLoging(string msg)
        {
            Console.WriteLine($"订阅者 1 从 队列{RedisMqKey.Loging} 消费到/接受到 消息:{msg}");

            await Task.CompletedTask;
        }
    }
}
