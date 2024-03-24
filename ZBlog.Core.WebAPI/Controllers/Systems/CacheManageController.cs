using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZBlog.Core.Common.Caches;
using ZBlog.Core.Common.GlobalVars;
using ZBlog.Core.Model;

namespace ZBlog.Core.WebAPI.Controllers.Systems
{
    /// <summary>
    /// 缓存管理
    /// </summary>
    [Route("api/Systems/[controller]")]
    [ApiController]
    [Authorize(Permissions.Name)]
    public class CacheManageController : BaseApiController
    {
        private readonly ICaching _caching;

        public CacheManageController(ICaching caching)
        {
            _caching = caching;
        }

        /// <summary>
        /// 获取全部缓存
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<List<string>>> Get()
        {
            return Success(await _caching.GetAllCacheKeysAsync());
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [HttpGet("{key}")]
        public async Task<MessageModel<string>> Get(string key)
        {
            return Success<string>(await _caching.GetStringAsync(key));
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expire"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MessageModel> Post([FromQuery] string key, [FromQuery] string value, [FromQuery] int? expire)
        {
            if (expire.HasValue)
                await _caching.SetStringAsync(key, value, TimeSpan.FromMilliseconds(expire.Value));
            else
                await _caching.SetStringAsync(key, value);

            return Success();
        }

        /// <summary>
        /// 删除全部缓存
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        public async Task<MessageModel> Delete()
        {
            await _caching.RemoveAllAsync();
            return Success();
        }

        /// <summary>
        /// 删除缓存
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [Route("{key}")]
        [HttpDelete]
        public async Task<MessageModel> Delete(string key)
        {
            await _caching.RemoveAsync(key);
            return Success();
        }
    }
}
