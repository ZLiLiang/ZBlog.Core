using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using ZBlog.Core.Common.Helper;
using ZBlog.Core.Common.Hubs;
using ZBlog.Core.Common.LogHelper;
using ZBlog.Core.Extensions.Middlewares;
using ZBlog.Core.IServices.IDS4Db;
using ZBlog.Core.Model;
using ZBlog.Core.Model.ViewModels;

namespace ZBlog.Core.WebAPI.Controllers
{
    [Route("api/[Controller]/[action]")]
    [ApiController]
    [AllowAnonymous]
    public class MonitorController : BaseApiController
    {
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IWebHostEnvironment _environment;
        private readonly IApplicationUserService _applicationUserService;
        private readonly ILogger<MonitorController> _logger;

        public MonitorController(IHubContext<ChatHub> hubContext,
                                 IWebHostEnvironment environment,
                                 IApplicationUserService applicationUserService,
                                 ILogger<MonitorController> logger)
        {
            _hubContext = hubContext;
            _environment = environment;
            _applicationUserService = applicationUserService;
            _logger = logger;
        }

        /// <summary>
        /// 服务器配置信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public MessageModel<ServerViewModel> Server()
        {
            return Success(new ServerViewModel
            {
                EnvironmentName = _environment.EnvironmentName,
                OSArchitecture = RuntimeInformation.OSArchitecture.ObjToString(),
                ContentRootPath = _environment.ContentRootPath,
                WebRootPath = _environment.WebRootPath,
                FrameworkDescription = RuntimeInformation.FrameworkDescription,
                WorkingTime = DateHelper.TimeSubTract(DateTime.Now, Process.GetCurrentProcess().StartTime)
            }, "获取服务器配置信息成功");
        }

        /// <summary>
        /// SignalR send data
        /// </summary>
        /// <returns></returns>
        // GET: api/Logs
        [HttpGet]
        public MessageModel<List<LogInfo>> Get()
        {
            if (AppSettings.App(new string[] { "Middleware", "SignalRSendLog", "Enabled" }).ObjToBool())
            {
                _hubContext.Clients.All.SendAsync("ReceiveUpdate", LogLock.GetLogData()).Wait();
            }

            return Success<List<LogInfo>>(null, "执行成功");
        }

        [HttpGet]
        public MessageModel<RequestApiWeekView> GetRequestApiinfoByWeek()
        {
            return Success(LogLock.RequestApiinfoByWeek(), "成功");
        }

        [HttpGet]
        public MessageModel<AccessApiDateView> GetAccessApiByDate()
        {
            return Success(LogLock.AccessApiByDate(), "获取成功");
        }

        [HttpGet]
        public MessageModel<AccessApiDateView> GetAccessApiByHour()
        {
            return Success(LogLock.AccessApiByHour(), "获取成功");
        }

        private List<UserAccessModel> GetAccessLogsToday(IWebHostEnvironment environment)
        {
            List<UserAccessModel> userAccessModels = new();
            var accessLogs = LogLock.ReadLog(Path.Combine(environment.ContentRootPath, "Log"), "RecordAccessLogs_", Encoding.UTF8, ReadType.PrefixLatest).ObjToString();

            try
            {
                return JsonConvert.DeserializeObject<List<UserAccessModel>>("[" + accessLogs + "]");
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
                            var accessItem = JsonConvert.DeserializeObject<UserAccessModel>(item.TrimEnd(','));
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

        private List<ActiveUserVM> GetAccessLogsTrend(IWebHostEnvironment environment)
        {
            List<ActiveUserVM> userAccessModels = new();
            var accessLogs = LogLock.ReadLog(
                Path.Combine(environment.ContentRootPath, "Log"), "ACCESSTRENDLOG_", Encoding.UTF8, ReadType.PrefixLatest
                ).ObjToString();
            try
            {
                return JsonConvert.DeserializeObject<List<ActiveUserVM>>(accessLogs);
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
                            var accessItem = JsonConvert.DeserializeObject<ActiveUserVM>(item.TrimStart('[').TrimEnd(']'));
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

        [HttpGet]
        public MessageModel<WelcomeInitData> GetActiveUsers([FromServices] IWebHostEnvironment environment)
        {
            var accessLogsToday = GetAccessLogsToday(environment).Where(d => d.BeginTime.ObjToDate() >= DateTime.Today);
            var logs = accessLogsToday.OrderByDescending(d => d.BeginTime)
                .Take(50)
                .ToList();
            var errorCountToday = LogLock.GetLogData()
                .Where(d => d.Import == 9)
                .Count();

            accessLogsToday = accessLogsToday.Where(d => d.User != "")
                .ToList();

            var activeUsers = accessLogsToday.GroupBy(it => new { it.User })
                .Select(it => new ActiveUserVM
                {
                    User = it.Key.User,
                    Count = it.Count()
                })
                .ToList();

            int activeUsersCount = activeUsers.Count;
            activeUsers = activeUsers.OrderByDescending(d => d.Count)
                .Take(10)
                .ToList();

            return Success(new WelcomeInitData
            {
                ActiveUsers = activeUsers,
                ActiveUserCount = activeUsersCount,
                ErrorCount = errorCountToday,
                Logs = logs,
                ActiveCount = GetAccessLogsTrend(environment)
            }, "获取成功");
        }

        [HttpGet]
        public async Task<MessageModel<AccessApiDateView>> GetIds4Users()
        {
            List<ApiDate> apiDates = new List<ApiDate>();

            if (_applicationUserService.IsEnable())
            {
                var users = await _applicationUserService.Query(d => d.TdIsDelete == false);

                apiDates = users.GroupBy(it => new { it.Birth.Date })
                    .Select(it => new ApiDate
                    {
                        Date = it.Key?.Date.ToString("yyyy-MM-dd"),
                        Count = it.Count()
                    })
                    .ToList();

                apiDates = apiDates.OrderByDescending(d => d.Date)
                    .Take(30)
                    .ToList();
            }

            if (apiDates.Count == 0)
            {
                apiDates.Add(new ApiDate
                {
                    Date = "没数据,或未开启相应接口服务",
                    Count = 0
                });
            }

            return Success(new AccessApiDateView
            {
                Columns = new string[] { "date", "count" },
                Rows = apiDates.OrderBy(d => d.Date)
                    .ToList()
            }, "获取成功");
        }
    }

    public class WelcomeInitData
    {
        public List<ActiveUserVM> ActiveUsers { get; set; }
        public int ActiveUserCount { get; set; }
        public List<UserAccessModel> Logs { get; set; }
        public int ErrorCount { get; set; }
        public List<ActiveUserVM> ActiveCount { get; set; }
    }
}
