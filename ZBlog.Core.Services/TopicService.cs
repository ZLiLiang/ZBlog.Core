using ZBlog.Core.Common.CustomAttribute;
using ZBlog.Core.IServices;
using ZBlog.Core.Model.Models;
using ZBlog.Core.Services.Base;

namespace ZBlog.Core.Services
{
    public class TopicService : BaseService<Topic>, ITopicService
    {
        /// <summary>
        /// 获取开Bug专题分类（缓存）
        /// </summary>
        /// <returns></returns>
        [Caching(AbsoluteExpiration = 60)]
        public async Task<List<Topic>> GetTopics()
        {
            return await base.Query(it => !it.IsDelete && it.SectendDetail == "bug");
        }
    }
}
