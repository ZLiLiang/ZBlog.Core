using Microsoft.Extensions.Caching.Distributed;

namespace ZBlog.Core.Common.Caches
{
    /// <summary>
    /// 缓存抽象接口,基于IDistributedCache封装
    /// </summary>
    public interface ICaching
    {
        /// <summary>
        /// 分布式缓存抽象接口
        /// </summary>
        public IDistributedCache Cache { get; }
        /// <summary>
        /// 增加缓存Key
        /// </summary>
        /// <param name="cacheKey"></param>
        void AddCacheKey(string cacheKey);
        /// <summary>
        /// 异步增加缓存Key
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        Task AddCacheKeyAsync(string cacheKey);

        /// <summary>
        /// 删除某特征关键字缓存
        /// </summary>
        /// <param name="key"></param>
        void DelByPattern(string key);
        /// <summary>
        /// 异步删除某特征关键字缓存
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task DelByPatternAsync(string key);

        /// <summary>
        /// 删除缓存
        /// </summary>
        /// <param name="cacheKey"></param>
        void DelCacheKey(string cacheKey);
        /// <summary>
        /// 异步删除缓存
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        Task DelCacheKeyAsync(string cacheKey);

        /// <summary>
        /// 检查给定 key 是否存在
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        bool Exists(string cacheKey);
        /// <summary>
        /// 异步检查给定 key 是否存在
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        Task<bool> ExistsAsync(string cacheKey);

        /// <summary>
        /// 获取所有缓存列表
        /// </summary>
        /// <returns></returns>
        List<string> GetAllCacheKeys();
        /// <summary>
        /// 异步获取所有缓存列表
        /// </summary>
        /// <returns></returns>
        Task<List<string>> GetAllCacheKeysAsync();

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        T Get<T>(string cacheKey);
        /// <summary>
        /// 异步获取缓存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        Task<T> GetAsync<T>(string cacheKey);

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="type"></param>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        object Get(Type type, string cacheKey);
        /// <summary>
        /// 异步获取缓存
        /// </summary>
        /// <param name="type"></param>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        Task<object> GetAsync(Type type, string cacheKey);

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        string GetString(string cacheKey);
        /// <summary>
        /// 异步获取缓存
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        Task<string> GetStringAsync(string cacheKey);

        /// <summary>
        /// 删除缓存
        /// </summary>
        /// <param name="key"></param>
        void Remove(string key);
        /// <summary>
        /// 异步删除缓存
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task RemoveAsync(string key);

        /// <summary>
        /// 删除所有缓存
        /// </summary>
        void RemoveAll();
        /// <summary>
        /// 异步删除所有缓存
        /// </summary>
        /// <returns></returns>
        Task RemoveAllAsync();

        /// <summary>
        /// 增加对象缓存,并设置过期时间
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="value"></param>
        /// <param name="expire"></param>
        void Set<T>(string cacheKey, T value, TimeSpan? expire = null);
        /// <summary>
        /// 异步增加对象缓存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        Task SetAsync<T>(string cacheKey, T value);
        /// <summary>
        /// 异步增加对象缓存,并设置过期时间
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="value"></param>
        /// <param name="expire"></param>
        /// <returns></returns>
        Task SetAsync<T>(string cacheKey, T value, TimeSpan expire);

        /// <summary>
        /// 增加某特征关键字缓存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="value"></param>
        void SetPermanent<T>(string cacheKey, T value);
        /// <summary>
        /// 异步增加某特征关键字缓存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        Task SetPermanentAsync<T>(string cacheKey, T value);

        /// <summary>
        /// 增加字符串缓存,并设置过期时间
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="value"></param>
        /// <param name="expire"></param>
        void SetString(string cacheKey, string value, TimeSpan? expire = null);
        /// <summary>
        /// 异步增加字符串缓存
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        Task SetStringAsync(string cacheKey, string value);
        /// <summary>
        /// 异步增加字符串缓存,并设置过期时间
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="value"></param>
        /// <param name="expire"></param>
        /// <returns></returns>
        Task SetStringAsync(string cacheKey, string value, TimeSpan expire);

        /// <summary>
        /// 异步根据父键清空
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task DelByParentKeyAsync(string key);
    }
}
