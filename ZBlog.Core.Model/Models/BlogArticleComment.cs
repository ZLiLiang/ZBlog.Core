using SqlSugar;
using ZBlog.Core.Model.Models.RootTkey;

namespace ZBlog.Core.Model.Models
{
    /// <summary>
    /// 博客文章 评论
    /// </summary>
    public class BlogArticleComment : RootEntityTkey<long>
    {
        public long MainId { get; set; }

        public string Comment { get; set; }

        public string UserId { get; set; }

        [Navigate(NavigateType.OneToOne, nameof(UserId))]
        public SysUserInfo User { get; set; }
    }
}
