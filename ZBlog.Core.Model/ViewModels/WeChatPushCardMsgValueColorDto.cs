namespace ZBlog.Core.Model.ViewModels
{
    /// <summary>
    /// 微信keyword所需Dto
    /// </summary>
    public class WeChatPushCardMsgValueColorDto
    {
        /// <summary>
        /// 内容
        /// </summary>
        public string Value { get; set; }
        /// <summary>
        /// 文字颜色
        /// </summary>
        public string Color { get; set; } = "#173177";
    }
}
