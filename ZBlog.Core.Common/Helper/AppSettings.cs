using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace ZBlog.Core.Common.Helper
{
    /// <summary>
    /// appsettings.json操作类
    /// </summary>
    public class AppSettings
    {
        private static IConfiguration _configuration;

        public AppSettings(string contentPath)
        {
            string path = "appsettings.json";
            //如果把配置文件 是 根据环境变量来分开了，可以这样写
            //$"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json";

            _configuration = new ConfigurationBuilder()
                .SetBasePath(contentPath)
                .Add(new JsonConfigurationSource
                {
                    Path = path,
                    Optional = false,
                    ReloadOnChange = true
                })  //这样的话，可以直接读目录里的json文件，而不是 bin 文件夹下的，所以不用修改复制属性
                .Build();
        }

        public AppSettings(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// 封装要操作的字符
        /// </summary>
        /// <param name="sections">节点配置</param>
        /// <returns></returns>
        public static string App(params string[] sections)
        {
            try
            {
                if (sections.Any())
                {
                    return _configuration[string.Join(":", sections)];
                }
            }
            catch (Exception)
            {

                throw;
            }

            return "";
        }

        /// <summary>
        /// 递归获取配置信息数组
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sections"></param>
        /// <returns></returns>
        public static List<T> App<T>(params string[] sections)
        {
            List<T> list = [];
            // 引用 Microsoft.Extensions.Configuration.Binder 包
            _configuration.Bind(string.Join(":", sections), list);

            return list;
        }

        /// <summary>
        /// 根据路径  configuration["App:Name"];
        /// </summary>
        /// <param name="sectionsPath"></param>
        /// <returns></returns>
        public static string GetValue(string sectionsPath)
        {
            try
            {
                return _configuration[sectionsPath];
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
