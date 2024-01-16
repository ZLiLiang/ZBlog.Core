namespace ZBlog.Core.Model.ViewModels
{
    public class WeChatPushVideoContentDto
    {
        /// <summary>
        /// 视频标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 视频封面mediaID
        /// </summary>
        public string PictureMediaID { get; set; }
        /// <summary>
        /// 视频mediaID
        /// </summary>
        public string VideoMediaID { get; set; }
    }
}
