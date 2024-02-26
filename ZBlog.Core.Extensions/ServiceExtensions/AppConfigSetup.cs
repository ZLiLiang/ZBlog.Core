using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ZBlog.Core.Common.DB;
using ZBlog.Core.Common.GlobalVars;
using ZBlog.Core.Common.Helper;
using ZBlog.Core.Common.Helper.Console.Table;

namespace ZBlog.Core.Extensions.ServiceExtensions
{
    /// <summary>
    /// 项目 启动服务
    /// </summary>
    public static class AppConfigSetup
    {
        public static void AddAppTableConfigSetup(this IServiceCollection services, IHostEnvironment environment)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (AppSettings.App(new string[] { "Startup", "AppConfigAlert", "Enabled" }).ObjToBool())
            {
                if (environment.IsDevelopment())
                {
                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                    Console.OutputEncoding = Encoding.GetEncoding("GB2312");
                }

                #region 程序配置

                List<string[]> configInfos = new()
                {
                    new string[] { "当前环境", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") },
                    new string[] { "当前的授权方案", Permissions.IsUseIds4 ? "Ids4" : "JWT" },
                    new string[] { "CORS跨域", AppSettings.App("Startup", "Cors", "EnableAllIPs") },
                    new string[] { "RabbitMQ消息列队", AppSettings.App("RabbitMQ", "Enabled") },
                    new string[] { "事件总线(必须开启消息列队)", AppSettings.App("EventBus", "Enabled") },
                    new string[] { "redis消息队列", AppSettings.App("Startup", "RedisMq", "Enabled") },
                    new string[] { "读写分离", BaseDBConfig.MainConfig.SlaveConnectionConfigs.AnyNoException()? "True" : "False" }
                };

                new ConsoleTable
                {
                    TitleString = "ZBlog.Core 配置集",
                    Columns = new string[] { "配置名称", "配置信息/是否启动" },
                    Rows = configInfos,
                    EnableCount = false,
                    Alignment = Alignment.Left,
                    ColumnBlankNum = 4,
                    TableStyle = TableStyle.Alternative
                }
                .Writer(ConsoleColor.Blue);
                Console.WriteLine();

                #endregion

                #region AOP

                List<string[]> aopInfos = new()
                {
                    new string[] { "缓存AOP", AppSettings.App("AppSettings", "CachingAOP", "Enabled") },
                    new string[] { "服务日志AOP", AppSettings.App("AppSettings", "LogAOP", "Enabled") },
                    new string[] { "事务AOP", AppSettings.App("AppSettings", "TranAOP", "Enabled") },
                    new string[] { "服务审计AOP", AppSettings.App("AppSettings", "UserAuditAOP", "Enabled") },
                    new string[] { "Sql执行AOP", AppSettings.App("AppSettings", "SqlAOP", "Enabled") },
                    new string[] { "Sql执行AOP控制台输出", AppSettings.App("AppSettings", "SqlAOP", "LogToConsole", "Enabled") }
                };

                new ConsoleTable
                {
                    TitleString = "AOP",
                    Columns = new string[] { "配置名称", "配置信息/是否启动" },
                    Rows = aopInfos,
                    EnableCount = false,
                    Alignment = Alignment.Left,
                    ColumnBlankNum = 7,
                    TableStyle = TableStyle.Alternative
                }
                .Writer(ConsoleColor.Blue);
                Console.WriteLine();

                #endregion

                #region 中间件

                List<string[]> middlewareInfos = new()
                {
                    new string[] { "请求纪录中间件", AppSettings.App("Middleware", "RecordAccessLogs", "Enabled") },
                    new string[] { "IP记录中间件", AppSettings.App("Middleware", "IPLog", "Enabled") },
                    new string[] { "请求响应日志中间件", AppSettings.App("Middleware", "RequestResponseLog", "Enabled") },
                    new string[] { "SingnalR实时发送请求数据中间件", AppSettings.App("Middleware", "SignalR", "Enabled") },
                    new string[] { "IP限流中间件", AppSettings.App("Middleware", "IpRateLimit", "Enabled") },
                    new string[] { "性能分析中间件", AppSettings.App("Startup", "MiniProfiler", "Enabled") },
                    new string[] { "Consul注册服务", AppSettings.App("Middleware", "Consul", "Enabled") }
                };

                new ConsoleTable
                {
                    TitleString = "中间件",
                    Columns = new string[] { "配置名称", "配置信息/是否启动" },
                    Rows = middlewareInfos,
                    EnableCount = false,
                    Alignment = Alignment.Left,
                    ColumnBlankNum = 3,
                    TableStyle = TableStyle.Alternative
                }.Writer(ConsoleColor.Blue);
                Console.WriteLine();


                #endregion
            }
        }
    }
}
