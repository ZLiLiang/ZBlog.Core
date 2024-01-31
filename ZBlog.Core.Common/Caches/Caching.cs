using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System.Text;
using ZBlog.Core.Common.Const;

namespace ZBlog.Core.Common.Caches
{
    public class Caching : ICaching
    {
        private readonly IDistributedCache _cache;

        public Caching(IDistributedCache cache)
        {
            _cache = cache;
        }

        public IDistributedCache Cache => _cache;

        private byte[] GetBytes<T>(T source)
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(source));
        }

        public void AddCacheKey(string cacheKey)
        {
            var res = _cache.GetString(CacheConst.KeyAll);
            var allKeys = string.IsNullOrWhiteSpace(res) ? new List<string>() : JsonConvert.DeserializeObject<List<string>>(res);
            if (!allKeys.Contains(cacheKey))
            {
                allKeys.Add(cacheKey);
                _cache.SetString(CacheConst.KeyAll, JsonConvert.SerializeObject(allKeys));
            }
        }

        public async Task AddCacheKeyAsync(string cacheKey)
        {
            var res = await _cache.GetStringAsync(CacheConst.KeyAll);
            var allKeys = string.IsNullOrWhiteSpace(res) ? new List<string>() : JsonConvert.DeserializeObject<List<string>>(res);
            if (!allKeys.Contains(cacheKey))
            {
                allKeys.Add(cacheKey);
                await _cache.SetStringAsync(CacheConst.KeyAll, JsonConvert.SerializeObject(allKeys));
            }
        }

        public void DelByPattern(string key)
        {
            var allKeys = GetAllCacheKeys();
            if (allKeys == null) return;

            var delAllKeys = allKeys.Where(k => k.Contains(key)).ToList();
            delAllKeys.ForEach(d => _cache.Remove(d));

            // 更新所有缓存键
            allKeys = allKeys.Where(k => !k.Contains(key)).ToList();
            _cache.SetString(CacheConst.KeyAll, JsonConvert.SerializeObject(allKeys));
        }

        public async Task DelByPatternAsync(string key)
        {
            var allKeys = await GetAllCacheKeysAsync();
            if (allKeys == null) return;

            var delAllKeys = allKeys.Where(k => k.Contains(key)).ToList();
            delAllKeys.ForEach(k => _cache.Remove(k));

            // 更新所有缓存键
            allKeys = allKeys.Where(k => !k.Contains(key)).ToList();
            await _cache.SetStringAsync(CacheConst.KeyAll, JsonConvert.SerializeObject(allKeys));
        }

        public void DelCacheKey(string cacheKey)
        {
            var res = _cache.GetString(CacheConst.KeyAll);
            var allKeys = string.IsNullOrWhiteSpace(res) ? new List<string>() : JsonConvert.DeserializeObject<List<string>>(res);
            if (allKeys.Contains(cacheKey))
            {
                allKeys.Remove(cacheKey);
                _cache.SetString(CacheConst.KeyAll, JsonConvert.SerializeObject(allKeys));
            }
        }

        public async Task DelCacheKeyAsync(string cacheKey)
        {
            var res = await _cache.GetStringAsync(CacheConst.KeyAll);
            var allKeys = string.IsNullOrWhiteSpace(res) ? new List<string>() : JsonConvert.DeserializeObject<List<string>>(res);
            if (allKeys.Contains(cacheKey))
            {
                allKeys.Remove(cacheKey);
                await _cache.SetStringAsync(CacheConst.KeyAll, JsonConvert.SerializeObject(allKeys));
            }
        }

        public bool Exists(string cacheKey)
        {
            var res = _cache.Get(cacheKey);
            return res != null;
        }

        public async Task<bool> ExistsAsync(string cacheKey)
        {
            var res = await _cache.GetAsync(cacheKey);
            return res != null;
        }

        public List<string> GetAllCacheKeys()
        {
            var res = _cache.GetString(CacheConst.KeyAll);
            var result = string.IsNullOrWhiteSpace(res) ? null : JsonConvert.DeserializeObject<List<string>>(res);

            return result;
        }

        public async Task<List<string>> GetAllCacheKeysAsync()
        {
            var res = await _cache.GetStringAsync(CacheConst.KeyAll);
            var result = string.IsNullOrWhiteSpace(res) ? null : JsonConvert.DeserializeObject<List<string>>(res);

            return result;
        }

        public T Get<T>(string cacheKey)
        {
            var value = _cache.Get(cacheKey);
            var result = value == null ? default : JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(value));

            return result;
        }

        public async Task<T> GetAsync<T>(string cacheKey)
        {
            var value = await _cache.GetAsync(cacheKey);
            var result = value == null ? default : JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(value));

            return result;
        }

        public object Get(Type type, string cacheKey)
        {
            var value = _cache.Get(cacheKey);
            var result = value == null ? default : JsonConvert.DeserializeObject(Encoding.UTF8.GetString(value), type);

            return result;
        }

        public async Task<object> GetAsync(Type type, string cacheKey)
        {
            var value = await _cache.GetAsync(cacheKey);
            var result = value == null ? default : JsonConvert.DeserializeObject(Encoding.UTF8.GetString(value), type);

            return result;
        }

        public string GetString(string cacheKey)
        {
            return _cache.GetString(cacheKey);
        }

        public async Task<string> GetStringAsync(string cacheKey)
        {
            return await _cache.GetStringAsync(cacheKey);
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
            DelCacheKey(key);
        }

        public async Task RemoveAsync(string key)
        {
            await _cache.RemoveAsync(key);
            await DelCacheKeyAsync(key);
        }

        public void RemoveAll()
        {
            var catches = GetAllCacheKeys();
            foreach (var cat in catches)
                Remove(cat);

            catches.Clear();
            _cache.SetString(CacheConst.KeyAll, JsonConvert.SerializeObject(catches));
        }

        public async Task RemoveAllAsync()
        {
            var catches = await GetAllCacheKeysAsync();
            foreach (var cat in catches)
                await RemoveAsync(cat);

            catches.Clear();
            await _cache.SetStringAsync(CacheConst.KeyAll, JsonConvert.SerializeObject(catches));
        }

        public void Set<T>(string cacheKey, T value, TimeSpan? expire = null)
        {
            _cache.Set(cacheKey,
                GetBytes(value),
                expire == null
                    ? new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6) }
                    : new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = expire });

            AddCacheKey(cacheKey);
        }

        public async Task SetAsync<T>(string cacheKey, T value)
        {
            await _cache.SetAsync(cacheKey,
                GetBytes(value),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6) });

            await AddCacheKeyAsync(cacheKey);
        }

        public async Task SetAsync<T>(string cacheKey, T value, TimeSpan expire)
        {
            await _cache.SetAsync(cacheKey,
                GetBytes(value),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = expire });

            await AddCacheKeyAsync(cacheKey);
        }

        public void SetPermanent<T>(string cacheKey, T value)
        {
            _cache.Set(cacheKey, GetBytes(value));
            AddCacheKey(cacheKey);
        }

        public async Task SetPermanentAsync<T>(string cacheKey, T value)
        {
            await _cache.SetAsync(cacheKey, GetBytes(value));
            await AddCacheKeyAsync(cacheKey);
        }

        public void SetString(string cacheKey, string value, TimeSpan? expire = null)
        {
            DistributedCacheEntryOptions options = null;
            if (expire == null)
                options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6) };
            else
                options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = expire };

            _cache.SetString(cacheKey, value, options);
            AddCacheKey(cacheKey);
        }

        public async Task SetStringAsync(string cacheKey, string value)
        {
            var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(6) };

            await _cache.SetStringAsync(cacheKey, value, options);
            await AddCacheKeyAsync(cacheKey);
        }

        public async Task SetStringAsync(string cacheKey, string value, TimeSpan expire)
        {
            var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = expire };

            await _cache.SetStringAsync(cacheKey, value, options);
            await AddCacheKeyAsync(cacheKey);
        }

        /// <summary>
        /// 缓存最大角色数据范围
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="dataScopeType"></param>
        /// <returns></returns>
        public async Task SetMaxDataScopeType(long userId, int dataScopeType)
        {
            var cacheKey = CacheConst.KeyMaxDataScopeType + userId;
            await SetStringAsync(cacheKey, dataScopeType.ToString());
            await AddCacheKeyAsync(cacheKey);
        }

        public async Task DelByParentKeyAsync(string key)
        {
            var allKeys = await GetAllCacheKeysAsync();
            if (allKeys == null) return;

            var delAllKeys = allKeys.Where(k => k.StartsWith(key)).ToList();
            delAllKeys.ForEach(Remove);

            // 更新所有缓存键
            allKeys = allKeys.Where(k => !k.StartsWith(key)).ToList();
            await SetStringAsync(CacheConst.KeyAll, JsonConvert.SerializeObject(allKeys));
        }
    }
}
