using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ZBlog.Core.Common.Helper;
using ZBlog.Core.IServices;
using ZBlog.Core.Tasks.QuartzNet;

namespace ZBlog.Core.Extensions.HostedService
{
    public class QuartzJobHostedService : IHostedService
    {
        private readonly ITasksQzService _tasksQzService;
        private readonly ISchedulerCenter _schedulerCenter;
        private readonly ILogger<QuartzJobHostedService> _logger;

        public QuartzJobHostedService(ITasksQzService tasksQzService, ISchedulerCenter schedulerCenter, ILogger<QuartzJobHostedService> logger)
        {
            _tasksQzService = tasksQzService;
            _schedulerCenter = schedulerCenter;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Start QuartzJob Service!");
            await DoWork();
        }

        private async Task DoWork()
        {
            try
            {
                if (AppSettings.App("Middleware", "QuartzNetJob", "Enabled").ObjToBool())
                {
                    var allQzServices = await _tasksQzService.Query();
                    foreach (var item in allQzServices)
                    {
                        if (item.IsStart)
                        {
                            var result = await _schedulerCenter.AddScheduleJobAsync(item);
                            if (result.Success)
                                Console.WriteLine($"QuartzNetJob{item.Name}启动成功！");
                            else
                                Console.WriteLine($"QuartzNetJob{item.Name}启动失败！错误信息：{result.Msg}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error was reported when starting the job service.");
                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stop QuartzJob Service!");
            return Task.CompletedTask;
        }
    }
}
