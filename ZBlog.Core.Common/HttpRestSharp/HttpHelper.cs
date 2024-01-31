using Newtonsoft.Json;
using RestSharp;
using System.Net;

namespace ZBlog.Core.Common.HttpRestSharp
{
    /// <summary>
    /// 基于 RestSharp 封装HttpHelper
    /// </summary>
    public static class HttpHelper
    {
        /// <summary>
        /// Get 请求
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="baseUrl">根域名:http://apk.neters.club/</param>
        /// <param name="url">接口:api/xx/yy</param>
        /// <param name="pragm">参数:id=2&name=老张</param>
        /// <returns></returns>
        public static T GetApi<T>(string baseUrl, string url, string pragm = "")
        {
            var client = new RestSharpClient(baseUrl);

            var response = client.Execute(string.IsNullOrEmpty(pragm)
                ? new RestRequest(url, Method.Get)
                : new RestRequest($"{url}?{pragm}", Method.Get));

            if (response.StatusCode != HttpStatusCode.OK)
                return (T)Convert.ChangeType(response.ErrorMessage, typeof(T));

            dynamic temp = JsonConvert.DeserializeObject(response.Content, typeof(T));

            return (T)temp;
        }

        /// <summary>
        /// Post 请求
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="baseUrl">根域名:http://apk.neters.club/</param>
        /// <param name="url">接口:api/xx/yy</param>
        /// <param name="body">post body,可以匿名或者反序列化</param>
        /// <returns></returns>
        public static T PostApi<T>(string baseUrl, string url, object body = null)
        {
            var client = new RestSharpClient(baseUrl);
            var request = new RestRequest(url, Method.Post);
            request.AddHeader("Accept", "application/json");
            request.AddJsonBody(body);

            var response = client.Execute(request);
            if (response.StatusCode != HttpStatusCode.OK)
                return (T)Convert.ChangeType(response.ErrorMessage, typeof(T));

            dynamic temp = JsonConvert.DeserializeObject(response.Content, typeof(T));

            return (T)temp;
        }
    }
}
