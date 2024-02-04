using ZBlog.Core.IServices.Base;
using ZBlog.Core.Model.Models;

namespace ZBlog.Core.IServices
{
    public interface ITopicDetailService : IBaseService<TopicDetail>
    {
        Task<List<TopicDetail>> GetTopicDetails();
    }
}
