using RestSharp;
using RestSharp.Authenticators;

namespace ZBlog.Core.Common.HttpRestSharp
{
    /// <summary>
    /// Rest接口执行者
    /// </summary>
    public class RestSharpClient : IRestSharp
    {
        /// <summary>
        /// 请求客户端
        /// </summary>
        private RestClient client;

        /// <summary>
        /// 接口基地址 格式：http://apk.neters.club/
        /// </summary>
        private string BaseUrl { get; set; }

        /// <summary>
        /// 默认的时间参数格式
        /// </summary>
        private string DefaultDateParameterFormat { get; set; }

        /// <summary>
        /// 默认验证器
        /// </summary>
        private IAuthenticator DefaultAuthenticator { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="baseUrl"></param>
        /// <param name="authenticator"></param>
        public RestSharpClient(string baseUrl, IAuthenticator authenticator = null)
        {
            BaseUrl = baseUrl;
            DefaultAuthenticator = authenticator;

            //默认时间显示格式
            DefaultDateParameterFormat = "yyyy-MM-dd HH:mm:ss";

            //配置基本url和默认校验器
            var options = new RestClientOptions(BaseUrl);
            if (DefaultAuthenticator != null) 
                options.Authenticator = DefaultAuthenticator;

            client = new RestClient(options, useClientFactory: true);
        }

        /// <summary>
        /// 通用执行方法
        /// </summary>
        /// <param name="request">请求参数</param>
        /// <remarks>
        /// 参数实例：<br/>
        ///  var request = new RestRequest(resource); // resource is the sub-path of the client base path
        ///  <br/> or <br/>
        ///  var request = new RestRequest(resource, Method.Post);
        /// </remarks>
        /// <returns></returns>
        public RestResponse Execute(RestRequest request)
        {
            var xmlRequest = request as RestXmlRequest;
            xmlRequest.DateFormat = string.IsNullOrEmpty(xmlRequest.DateFormat) ? DefaultDateParameterFormat : xmlRequest.DateFormat;

            var response = client.Execute(xmlRequest);

            return response;
        }

        /// <summary>
        /// 同步执行方法
        /// </summary>
        /// <typeparam name="T">返回的泛型对象</typeparam>
        /// <param name="request">请求参数</param>
        /// <remarks>
        /// 参数实例：<br/>
        ///  var request = new RestRequest(resource); // resource is the sub-path of the client base path
        ///  <br/> or <br/>
        ///  var request = new RestRequest(resource, Method.Post);
        /// </remarks>
        /// <returns></returns>
        public RestResponse<T> Execute<T>(RestRequest request) where T : new()
        {
            var xmlRequest = request as RestXmlRequest;
            xmlRequest.DateFormat = string.IsNullOrEmpty(xmlRequest.DateFormat) ? DefaultDateParameterFormat : xmlRequest.DateFormat;

            var response = client.Execute<T>(xmlRequest);

            return response;
        }

        /// <summary>
        /// 异步执行方法
        /// </summary>
        /// <param name="request">请求参数</param>
        /// <remarks>
        /// 参数实例：<br/>
        ///  var request = new RestRequest(resource); // resource is the sub-path of the client base path
        ///  <br/> or <br/>
        ///  var request = new RestRequest(resource, Method.Post);
        /// </remarks>
        /// <returns></returns>
        public Task<RestResponse> ExecuteAsync(RestRequest request)
        {
            var xmlRequest = request as RestXmlRequest;
            xmlRequest.DateFormat = string.IsNullOrEmpty(xmlRequest.DateFormat) ? DefaultDateParameterFormat : xmlRequest.DateFormat;

            var response = client.ExecuteAsync(xmlRequest);

            return response;
        }

        /// <summary>
        /// 异步执行方法
        /// </summary>
        /// <typeparam name="T">返回的泛型对象</typeparam>
        /// <param name="request">请求参数</param>
        /// <remarks>
        /// 参数实例：<br/>
        ///  var request = new RestRequest(resource); // resource is the sub-path of the client base path
        ///  <br/> or <br/>
        ///  var request = new RestRequest(resource, Method.Post);
        /// </remarks>
        /// <returns></returns>
        public Task<RestResponse<T>> ExecuteAsync<T>(RestRequest request) where T : new()
        {
            var xmlRequest = request as RestXmlRequest;
            xmlRequest.DateFormat = string.IsNullOrEmpty(xmlRequest.DateFormat) ? DefaultDateParameterFormat : xmlRequest.DateFormat;

            var response = client.ExecuteAsync<T>(xmlRequest);

            return response;
        }
    }
}
