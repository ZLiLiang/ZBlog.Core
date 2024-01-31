using Nacos.V2;
using System.Net.Http.Headers;
using System.Text;
using ZBlog.Core.Common.Helper;

namespace ZBlog.Core.Common.Extensions
{
    /// <summary>
    /// Linq扩展
    /// </summary>
    public static class NacosExpressionExtension
    {
        #region Nacos NamingService

        private static string GetServiceUrl(INacosNamingService service, string serviceName, string group,
            string apiUrl)
        {
            try
            {
                var instance = service.SelectOneHealthyInstance(serviceName, group).GetAwaiter().GetResult();
                var host = $"{instance.Ip}:{instance.Port}";
                if (instance.Metadata.ContainsKey("endpoint"))
                    host = instance.Metadata["endpoint"];

                var baseUrl = instance.Metadata.TryGetValue("secure", out _)
                    ? $"https://{host}"
                    : $"http://{host}";

                if (string.IsNullOrWhiteSpace(baseUrl)) return "";

                return $"{baseUrl}{apiUrl}";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "";
            }
        }

        public static async Task<string> Cof_NaoceGet(this INacosNamingService service, string serviceName, string group, string apiUrl, Dictionary<string, string> parameters = null)
        {
            try
            {
                var url = GetServiceUrl(service, serviceName, group, apiUrl);
                if (string.IsNullOrEmpty(url))
                    return "";
                if (parameters != null && parameters.Any())
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var item in parameters)
                    {
                        sb.Append($"{item.Key}={item.Value}");
                    }

                    url = $"{url}?{sb.ToString().Trim('&')}";
                }

                HttpHelper.HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var result = await HttpHelper.HttpClient.GetAsync(url);

                return await result.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "";
            }
        }

        public static async Task<string> Cof_NaocePostForm(INacosNamingService service, string serviceName, string group, string apiUrl, Dictionary<string, string> parameters)
        {
            try
            {
                var url = GetServiceUrl(service, serviceName, group, apiUrl);
                if (string.IsNullOrEmpty(url)) return "";

                var content = (parameters != null && parameters.Any()) ? new FormUrlEncodedContent(parameters) : null;
                HttpHelper.HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var result = await HttpHelper.HttpClient.PostAsync(url, content);

                return await result.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                return "";
            }
        }

        public static async Task<string> Cof_NaocePostJson(this INacosNamingService service, string serviceName, string group, string apiUrl, string jsonData)
        {
            try
            {
                var url = GetServiceUrl(service, serviceName, group, apiUrl);
                if (string.IsNullOrEmpty(url)) return "";

                HttpHelper.HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var result = await HttpHelper.HttpClient.PostAsync(url, new StringContent(jsonData, Encoding.UTF8, "application/json"));

                return await result.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                return "";
            }
        }

        public static async Task<string> Cof_NaocePostFile(this INacosNamingService service, string serviceName, string group, string apiUrl, Dictionary<string, byte[]> parameters)
        {
            try
            {
                var url = GetServiceUrl(service, serviceName, group, apiUrl);
                if (string.IsNullOrEmpty(url)) return "";

                var content = new MultipartFormDataContent();
                foreach (var item in parameters)
                {
                    content.Add(new ByteArrayContent(item.Value), "files", item.Key);
                }

                HttpHelper.HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var result = await HttpHelper.HttpClient.PostAsync(url, content);

                return await result.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                return "";
            }
        }

        #endregion
    }
}
