using System.Text;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using Quartz;
using ZBlog.Core.Common.Helper;
using ZBlog.Core.Common.LogHelper;
using ZBlog.Core.IServices;
using ZBlog.Core.Model.Models;

//Tip:这里要注意下，命名空间和程序集是一样的，不然反射不到
namespace ZBlog.Core.Tasks
{
    public class JobAccessTrendLogQuartz : JobBase, IJob
    {
        private readonly IAccessTrendLogService _accessTrendLogService;
        private readonly IWebHostEnvironment _environment;

        public JobAccessTrendLogQuartz(IAccessTrendLogService accessTrendLogService,
            IWebHostEnvironment environment,
            ITasksQzService tasksQzService,
            ITasksLogService tasksLogService) :
            base(tasksQzService, tasksLogService)
        {
            _accessTrendLogService = accessTrendLogService;
            _environment = environment;
            _tasksQzService = tasksQzService;
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

            var latestLogDatetime = (await _accessTrendLogService.Query(null, d => d.UpdateTime, false)).FirstOrDefault()?.UpdateTime;
            if (latestLogDatetime == null)
                latestLogDatetime = Convert.ToDateTime("2021-09-01");

            var accessLogs = GetAccessLogs()
                .Where(d => d.User != "" && d.BeginTime.ObjToDate() >= latestLogDatetime)
                .ToList();
            var logUpdate = DateTime.Now;

            var activeUsers = accessLogs.GroupBy(it => new { it.User })
                .Select(it => new ActiveUserVM
                {
                    User = it.Key.User,
                    Count = it.Count()
                })
                .ToList();

            foreach (var item in activeUsers)
            {
                var user = (await _accessTrendLogService.Query(it => it.UserInfo != "" && it.UserInfo == item.User)).FirstOrDefault();
                if (user != null)
                {
                    user.Count += item.Count;
                    user.UpdateTime = logUpdate;
                    await _accessTrendLogService.Update(user);
                }
                else
                {
                    await _accessTrendLogService.Add(new AccessTrendLog
                    {
                        Count = item.Count,
                        UpdateTime = logUpdate,
                        UserInfo = item.User
                    });
                }
            }

            // 重新拉取
            var actUsers = (await _accessTrendLogService.Query(it => it.UserInfo != "", it => it.Count, false))
                .Take(15)
                .ToList();

            List<ActiveUserVM> activeUserVMs = new();
            foreach (var item in actUsers)
            {
                activeUserVMs.Add(new ActiveUserVM
                {
                    User = item.UserInfo,
                    Count = item.Count
                });
            }

            Parallel.For(0, 1, e =>
            {
                LogLock.OutLogAOP("ACCESSTRENDLOG", "", new string[] { activeUserVMs.GetType().ToString(), JsonConvert.SerializeObject(activeUserVMs) }, false);
            });
        }

        private List<UserAccessFromFiles> GetAccessLogs()
        {
            List<UserAccessFromFiles> userAccessModels = new();
            var accessLogs = LogLock.ReadLog(Path.Combine(_environment.ContentRootPath, "Log"),
                "RecordAccessLogs_",
                Encoding.UTF8,
                ReadType.Prefix, 2)
                .ObjToString()
                .TrimEnd(',');

            try
            {
                return JsonConvert.DeserializeObject<List<UserAccessFromFiles>>($"[{accessLogs}]");
            }
            catch (Exception)
            {
                var accessLogArr = accessLogs.Split("\n");
                foreach (var item in accessLogArr)
                {
                    if (item.ObjToString() != "")
                    {
                        try
                        {
                            var accessItem = JsonConvert.DeserializeObject<UserAccessFromFiles>(item.TrimEnd(','));
                            userAccessModels.Add(accessItem);
                        }
                        catch (Exception)
                        {

                        }
                    }
                }
            }

            return userAccessModels;
        }
    }

    public class UserAccessFromFiles
    {
        public string User { get; set; }
        public string IP { get; set; }
        public string API { get; set; }
        public string BeginTime { get; set; }
        public string OPTime { get; set; }
        public string RequestMethod { get; set; } = "";
        public string Agent { get; set; }
    }
}
