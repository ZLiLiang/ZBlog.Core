using Microsoft.Extensions.Hosting;
using ZBlog.Core.Common.Helper.Console;

namespace ZBlog.Core.Extensions.HostedService
{
    public class Job2TimedService : IHostedService, IDisposable
    {
        private Timer _timer;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Job 2 is starting.");

            _timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromSeconds(60 * 60 * 2));//两个小时

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            ConsoleHelper.WriteWarningLine($"Job 2： {DateTime.Now}");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Job 2 is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
