using MapsterMapper;
using ZBlog.Core.Common.CustomAttribute;
using ZBlog.Core.IServices;
using ZBlog.Core.Model.Models;
using ZBlog.Core.Model.ViewModels;
using ZBlog.Core.Services.Base;

namespace ZBlog.Core.Services
{
    public class BlogArticleService : BaseService<BlogArticle>, IBlogArticleService
    {
        IMapper _mapper;

        public BlogArticleService(IMapper mapper)
        {
            _mapper = mapper;
        }

        /// <summary>
        /// 获取博客列表
        /// </summary>
        /// <returns></returns>
        [Caching(AbsoluteExpiration = 10)]
        public async Task<List<BlogArticle>> GetBlogs()
        {
            var blogs = await base.Query(a => a.Id > 0, a => a.Id);

            return blogs;
        }

        /// <summary>
        /// 获取视图博客详情信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<BlogViewModel> GetBlogDetails(long id)
        {
            var blogArticle = (await base.Query(a => a.Id == id && a.Category == "技术博文")).FirstOrDefault();

            BlogViewModel models = null;

            if (blogArticle != null)
            {
                models = _mapper.Map<BlogViewModel>(blogArticle);

                //要取下一篇和上一篇，以当前id开始，按id排序后top(2)，而不用取出所有记录
                //这样在记录很多的时候也不会有多大影响
                var nextBlogs = await base.Query(a => a.Id >= id && a.IsDeleted == false && a.Category == "技术博文", 2, "Id");
                if (nextBlogs.Count == 2)
                {
                    models.Next = nextBlogs[1].Title;
                    models.NextId = nextBlogs[1].Id;
                }

                var prevBlogs = await base.Query(a => a.Id <= id && a.IsDeleted == false && a.Category == "技术博文", 2, "Id desc");
                if (prevBlogs.Count == 2)
                {
                    models.Previous = prevBlogs[1].Title;
                    models.PreviousId = prevBlogs[1].Id;
                }

                blogArticle.Traffic += 1;
                await base.Update(blogArticle, new List<string> { "Traffic" });
            }

            return models;
        }
    }
}
