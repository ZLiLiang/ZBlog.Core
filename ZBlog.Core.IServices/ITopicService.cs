using ZBlog.Core.IServices.Base;
using ZBlog.Core.Model.Models;

namespace ZBlog.Core.IServices
{
    public interface ITopicService : IBaseService<Topic>
    {
        Task<List<Topic>> GetTopics();
    }
}
