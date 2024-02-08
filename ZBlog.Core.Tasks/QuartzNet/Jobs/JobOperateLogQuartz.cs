using System.Text;
using Microsoft.AspNetCore.Hosting;
using Quartz;
using ZBlog.Core.Common.Helper;
using ZBlog.Core.Common.LogHelper;
using ZBlog.Core.IServices;
using ZBlog.Core.Model.Models;

/// <summary>
/// 这里要注意下，命名空间和程序集是一样的，不然反射不到
/// </summary>
namespace ZBlog.Core.Tasks
{
    public class JobOperateLogQuartz : JobBase, IJob
    {
        private readonly IOperateLogService _operateLogService;
        private readonly IWebHostEnvironment _environment;

        public JobOperateLogQuartz(ITasksQzService tasksQzService,
            ITasksLogService tasksLogService,
            IOperateLogService operateLogService,
            IWebHostEnvironment environment) :
            base(tasksQzService, tasksLogService)
        {
            _operateLogService = operateLogService;
            _environment = environment;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var executeLog = await ExecuteJob(context, async () => await Run(context));
        }

        public async Task Run(IJobExecutionContext context)
        {
            // 可以直接获取 JobDetail 的值
            var jobKey = context.JobDetail.Key;
            var jobId = jobKey.Name;
            // 也可以通过数据库配置，获取传递过来的参数
            JobDataMap data = context.JobDetail.JobDataMap;

            List<LogInfo> excLogs = new List<LogInfo>();
            var excLogContent = LogLock.ReadLog(Path.Combine(_environment.ContentRootPath, "Log"), $"GlobalExceptionLogs_{DateTime.Now.ToString("yyyMMdd")}.log", Encoding.UTF8);

            if (!string.IsNullOrEmpty(excLogContent))
            {
                excLogs = excLogContent.Split("--------------------------------")
                    .Where(it => !string.IsNullOrEmpty(it) && it != "\n" && it != "\r\n")
                    .Select(it => new LogInfo
                    {
                        Datetime = (it.Split("|")[0]).Split(',')[0].ObjToDate(),
                        Content = it.Split("|")[1]?.Replace("\r\n", "<br>"),
                        LogColor = "EXC",
                        Import = 9
                    })
                    .ToList();
            }

            var filterDatetime = DateTime.Now.AddHours(-1);
            excLogs = excLogs.Where(it => it.Datetime >= filterDatetime)
                .ToList();

            var operateLogs = new List<OperateLog>() { };
            excLogs.ForEach(it =>
            {
                operateLogs.Add(new OperateLog
                {
                    LogTime = it.Datetime,
                    Description = it.Content,
                    IPAddress = it.IP,
                    UserId = 0,
                    IsDeleted = false
                });
            });

            if (operateLogs.Count > 0)
            {
                var logsIds = await _operateLogService.Add(operateLogs);
            }
        }
    }
}
