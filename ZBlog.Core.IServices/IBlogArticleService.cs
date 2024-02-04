using ZBlog.Core.IServices.Base;
using ZBlog.Core.Model.Models;
using ZBlog.Core.Model.ViewModels;

namespace ZBlog.Core.IServices
{
    public interface IBlogArticleService : IBaseService<BlogArticle>
    {
        Task<List<BlogArticle>> GetBlogs();
        Task<BlogViewModel> GetBlogDetails(long id);
    }
}
