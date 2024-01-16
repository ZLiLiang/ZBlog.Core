namespace ZBlog.Core.Model.ViewModels
{
    public class WeChatPushLinkMsgContentDto
    {
        /// <summary>
        /// 图文链接标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 图文描述
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 访问URL
        /// </summary>
        public string ViewUrl { get; set; }
        /// <summary>
        /// 图片URL
        /// </summary>
        public string PictureUrl { get; set; }
    }
}
