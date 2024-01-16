using SqlSugar;
using ZBlog.Core.Model.Models.RootTkey;

namespace ZBlog.Core.Model.Models
{
    public class Guestbook : RootEntityTkey<long>
    {
        /// <summary>
        /// 博客ID
        /// </summary>
        public long? BlogId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 用户名字
        /// </summary>
        [SugarColumn(Length = 2000, IsNullable = true)]
        public string UserName { get; set; }

        /// <summary>
        /// 手机
        /// </summary>
        [SugarColumn(Length = 2000, IsNullable = true)]
        public string Phone { get; set; }

        /// <summary>
        /// QQ
        /// </summary>
        [SugarColumn(Length = 2000, IsNullable = true)]
        public string QQ { get; set; }

        /// <summary>
        /// 留言内容
        /// </summary>
        [SugarColumn(Length = 2000, IsNullable = true)]
        public string Body { get; set; }

        /// <summary>
        /// Ip地址
        /// </summary>
        [SugarColumn(Length = 2000, IsNullable = true)]
        public string Ip { get; set; }

        /// <summary>
        /// 是否显示在前台,0否1是
        /// </summary>
        public bool IsShow { get; set; }

        /// <summary>
        /// 博客文章
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public BlogArticle BlogArticle { get; set; }
    }
}
