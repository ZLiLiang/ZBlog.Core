using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ZBlog.Core.Common.Helper
{
    /// <summary>
    /// Json 配置文件通用类
    /// </summary>
    public static class JsonConfigUtils
    {
        #region 变量

        /// <summary>
        /// 锁
        /// </summary>
        private static object _lock = new object();

        #endregion

        /// <summary>
        /// 读取配置文件的信息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configuration"></param>
        /// <param name="appSettingsFileName">要读取json的名称</param>
        /// <param name="key">要读取的json节点名称</param>
        /// <returns></returns>
        public static T GetAppSettings<T>(IConfiguration configuration, string appSettingsFileName, string key) where T : class, new()
        {
            lock (_lock)
            {
                // 如果为空，则为其赋值
                configuration ??= new ConfigurationBuilder()
                        .Add(new JsonConfigurationSource
                        {
                            Path = appSettingsFileName,
                            Optional = false,
                            ReloadOnChange = true
                        })
                        .Build();
                var appconfig = new ServiceCollection()
                    .AddOptions()
                    .Configure<T>(configuration.GetSection(key))
                    .BuildServiceProvider()
                    .GetService<IOptions<T>>()
                    .Value;

                return appconfig;
            }
        }

        public static string GetJson(string jsonPath, string key)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile(jsonPath)  //json文件地址
                .Build();
            string result = configuration.GetSection(key).Value;    //json某个对象

            return result;
        }
    }

    #region Nacos 配置清单

    public class JsonConfigSettings
    {
        /// <summary>
        /// 从nacos 读取到的系统配置信息
        /// </summary>
        public static IConfiguration Configuration { get; set; }

        /// <summary>
        /// 配置文件名称常量
        /// </summary>
        private static readonly string appSettingsFileName = $"appsettings{GetAppSettingsConfigName()}json";

        /// <summary>
        /// 根据环境变量定向配置文件名称
        /// </summary>
        /// <returns></returns>
        private static string GetAppSettingsConfigName()
        {
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != null && Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "")
            {
                return $".{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.";
            }
            else
            {
                return ".";
            }
        }

        /// <summary>
        /// 获取Nacos配置
        /// </summary>
        public static List<string> NacosServerAddresses
        {
            get
            {
                return JsonConfigUtils.GetAppSettings<NacosConfigDTO>(Configuration, appSettingsFileName, "nacos").ServerAddresses;
            }
        }

        /// <summary>
        /// 获取Nacos配置
        /// </summary>
        public static int NacosDefaultTimeOut
        {
            get
            {
                return JsonConfigUtils.GetAppSettings<NacosConfigDTO>(Configuration, appSettingsFileName, "nacos").DefaultTimeOut;
            }
        }

        /// <summary>
        /// 获取Nacos配置
        /// </summary>
        public static string NacosNamespace
        {
            get
            {
                return JsonConfigUtils.GetAppSettings<NacosConfigDTO>(Configuration, appSettingsFileName, "nacos").Namespace;
            }
        }

        /// <summary>
        /// 获取Nacos配置
        /// </summary>
        public static string NacosServiceName
        {
            get
            {
                return JsonConfigUtils.GetAppSettings<NacosConfigDTO>(Configuration, appSettingsFileName, "nacos").ServiceName;
            }
        }

        /// <summary>
        /// 获取Nacos配置
        /// </summary>
        public static int ListenInterval
        {
            get
            {
                return JsonConfigUtils.GetAppSettings<NacosConfigDTO>(Configuration, appSettingsFileName, "nacos").ListenInterval;
            }
        }

        /// <summary>
        /// 获取Nacos配置
        /// </summary>
        public static string NacosIp
        {
            get
            {
                return JsonConfigUtils.GetAppSettings<NacosConfigDTO>(Configuration, appSettingsFileName, "nacos").Ip;

            }
        }

        /// <summary>
        /// 获取Nacos配置
        /// </summary>
        public static int NacosPort
        {
            get
            {
                return int.Parse(JsonConfigUtils.GetAppSettings<NacosConfigDTO>(Configuration, appSettingsFileName, "nacos").Port);
            }
        }

        /// <summary>
        /// 获取Nacos配置
        /// </summary>
        public static bool NacosRegisterEnabled
        {
            get
            {
                return JsonConfigUtils.GetAppSettings<NacosConfigDTO>(Configuration, appSettingsFileName, "nacos").RegisterEnabled;
            }
        }

        /// <summary>
        /// 获取Nacos配置
        /// </summary>
        public static Dictionary<string, string> NacosMetadata
        {
            get
            {
                return JsonConfigUtils.GetAppSettings<NacosConfigDTO>(Configuration, appSettingsFileName, "nacos").Metadata;
            }
        }
    }

    #endregion

    #region Nacos配置

    class NacosConfigDTO
    {
        /// <summary>
        /// 服务IP地址
        /// </summary>
        public List<string> ServerAddresses { get; set; }
        /// <summary>
        /// 默认超时时间
        /// </summary>
        public int DefaultTimeOut { get; set; }
        /// <summary>
        /// 监听间隔
        /// </summary>
        public int ListenInterval { get; set; }
        /// <summary>
        /// 服务命名空间
        /// </summary>
        public string Namespace { get; set; }
        /// <summary>
        /// 服务名称
        /// </summary>
        public string ServiceName { get; set; }
        /// <summary>
        /// IP地址
        /// </summary>
        public string Ip { get; set; }
        /// <summary>
        /// 端口
        /// </summary>
        public string Port { get; set; }
        /// <summary>
        /// 服务命名空间
        /// </summary>
        public bool RegisterEnabled { get; set; }
        /// <summary>
        /// 其他配置
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; }
    }

    #endregion
}
