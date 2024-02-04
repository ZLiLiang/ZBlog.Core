using ZBlog.Core.Common.CustomAttribute;
using ZBlog.Core.IServices;
using ZBlog.Core.Model.Models;
using ZBlog.Core.Services.Base;

namespace ZBlog.Core.Services
{
    public class TopicDetailService : BaseService<TopicDetail>, ITopicDetailService
    {
        /// <summary>
        /// 获取开Bug数据（缓存）
        /// </summary>
        /// <returns></returns>
        [Caching(AbsoluteExpiration = 10)]
        public async Task<List<TopicDetail>> GetTopicDetails()
        {
            return await base.Query(a => !a.IsDelete && a.SectendDetail == "bug");
        }
    }
}
