using Microsoft.Extensions.Hosting;
using ZBlog.Core.Common.Helper.Console;
using ZBlog.Core.IServices;

namespace ZBlog.Core.Tasks.HostedService
{
    public class Job1TimedService : IHostedService, IDisposable
    {
        private Timer _timer;
        private readonly IBlogArticleService _blogArticleService;

        public Job1TimedService(IBlogArticleService blogArticleService)
        {
            _blogArticleService = blogArticleService;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Job 1 is starting.");

            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(60 * 60));//一个小时

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            try
            {
                var model = _blogArticleService.GetBlogDetails(1).Result;
                Console.WriteLine($"Job 1 启动成功，获取id=1的博客title为:{model?.Title}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error:{ex.Message}");
            }

            ConsoleHelper.WriteSuccessLine($"Job 1： {DateTime.Now}");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Job 1 is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
