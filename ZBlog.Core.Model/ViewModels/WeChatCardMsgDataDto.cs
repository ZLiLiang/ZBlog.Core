namespace ZBlog.Core.Model.ViewModels
{
    /// <summary>
    /// 微信推送消息Dto
    /// </summary>
    public class WeChatCardMsgDataDto
    {
        /// <summary>
        /// 推送关键信息
        /// </summary>
        public WeChatUserInfo Info { get; set; }
        /// <summary>
        /// 推送卡片消息Dto
        /// </summary>
        public WeChatCardMsgDetailDto CardMsg { set; get; }
    }
}
