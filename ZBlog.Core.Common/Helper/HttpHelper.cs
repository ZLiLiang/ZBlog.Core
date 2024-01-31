using System.Net.Http.Headers;

namespace ZBlog.Core.Common.Helper
{
    /// <summary>
    /// httpclinet请求方式，请尽量使用IHttpClientFactory方式
    /// </summary>
    public class HttpHelper
    {
        public static readonly HttpClient HttpClient = new();

        public static async Task<string> GetAsync(string serviceAddress)
        {
            try
            {
                Uri getUrl = new Uri(serviceAddress);
                HttpClient.Timeout = new TimeSpan(0, 0, 60);
                string result = await HttpClient.GetAsync(getUrl).Result.Content.ReadAsStringAsync();

                return result;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }

            return null;
        }

        public static async Task<string> PostAsync(string serviceAddress, string requestJson = null)
        {
            try
            {
                Uri postUrl = new Uri(serviceAddress);
                using (HttpContent httpContent = new StringContent(requestJson))
                {
                    httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    HttpClient.Timeout = new TimeSpan(0, 0, 60);
                    string result = await HttpClient.PostAsync(postUrl, httpContent).Result.Content.ReadAsStringAsync();

                    return result;
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }

            return null;
        }
    }
}
