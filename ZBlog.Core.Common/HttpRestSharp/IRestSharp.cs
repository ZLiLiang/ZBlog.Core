using RestSharp;

namespace ZBlog.Core.Common.HttpRestSharp
{
    /// <summary>
    /// API请求执行者接口
    /// </summary>
    public interface IRestSharp
    {
        /// <summary>
        /// 同步执行方法
        /// </summary>
        /// <param name="request">请求参数</param>
        /// <returns></returns>
        RestResponse Execute(RestRequest request);

        /// <summary>
        /// 同步执行方法
        /// </summary>
        /// <typeparam name="T">返回值</typeparam>
        /// <param name="request">请求参数</param>
        /// <returns></returns>
        RestResponse<T> Execute<T>(RestRequest request) where T : new();

        /// <summary>
        /// 异步执行方法
        /// </summary>
        /// <param name="request">请求参数</param>
        /// <returns></returns>
        Task<RestResponse> ExecuteAsync(RestRequest request);

        /// <summary>
        /// 异步执行方法
        /// </summary>
        /// <typeparam name="T">返回值</typeparam>
        /// <param name="request">请求参数</param>
        /// <returns></returns>
        Task<RestResponse<T>> ExecuteAsync<T>(RestRequest request) where T : new();
    }
}
