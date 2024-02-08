using Microsoft.Extensions.Logging;
using Quartz;
using ZBlog.Core.Common.Helper;
using ZBlog.Core.IServices;

/// <summary>
/// 这里要注意下，命名空间和程序集是一样的，不然反射不到(任务类要去JobSetup添加注入)
/// </summary>
namespace ZBlog.Core.Tasks
{
    public class JobURLQuartz : JobBase, IJob
    {
        private readonly ILogger<JobURLQuartz> _logger;

        public JobURLQuartz(ITasksQzService tasksQzService, ITasksLogService tasksLogService, ILogger<JobURLQuartz> logger) : base(tasksQzService, tasksLogService)
        {
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            // 可以直接获取 JobDetail 的值
            var jobKey = context.JobDetail.Key;
            var jobId = jobKey.Name;
            var executeLog = await ExecuteJob(context, async () => await Run(context, jobId.ObjToInt()));
        }

        public async Task Run(IJobExecutionContext context, int jobId)
        {
            if (jobId > 0)
            {
                JobDataMap data = context.JobDetail.JobDataMap;
                string pars = data.GetString("JobParam");
                if (!string.IsNullOrWhiteSpace(pars))
                {
                    var log = await HttpHelper.GetAsync(pars);
                    _logger.LogInformation(log);
                }
            }
        }
    }
}
