using SqlSugar;
using ZBlog.Core.Model.Models.RootTkey;

namespace ZBlog.Core.Model.Models
{
    /// <summary>
    /// 博客文章
    /// </summary>
    public class BlogArticle : RootEntityTkey<long>
    {
        /// <summary>
        /// 创建人
        /// </summary>
        [SugarColumn(Length = 600, IsNullable = true)]
        public string Submitter { get; set; }

        [Navigate(NavigateType.OneToOne, nameof(Submitter))]
        public SysUserInfo User { get; set; }

        /// <summary>
        /// 标题blog
        /// </summary>
        [SugarColumn(Length = 256, IsNullable = true)]
        public string Title { get; set; }

        /// <summary>
        /// 类别
        /// </summary>
        [SugarColumn(Length = 2000, IsNullable = true)]
        public string Category { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        [SugarColumn(Length = 2000, IsNullable = true)]
        public string Content { get; set; }

        /// <summary>
        /// 访问量
        /// </summary>
        public int Traffic { get; set; }

        /// <summary>
        /// 评论数量
        /// </summary>
        public int CommentNum { get; set; }

        /// <summary> 
        /// 修改时间
        /// </summary>
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [SugarColumn(Length = 2000, IsNullable = true)]
        public string Remark { get; set; }

        /// <summary>
        /// 逻辑删除
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public bool? IsDeleted { get; set; }

        /// <summary>
        /// 评论
        /// </summary>
        [Navigate(NavigateType.OneToMany, nameof(BlogArticleComment.MainId))]
        public List<BlogArticleComment> Comments { get; set; }
    }
}
