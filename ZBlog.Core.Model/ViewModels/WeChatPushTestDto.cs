namespace ZBlog.Core.Model.ViewModels
{
    /// <summary>
    /// 推送模拟消息Dto
    /// </summary>
    public class WeChatPushTestDto
    {
        /// <summary>
        /// 当前选中的微信公众号
        /// </summary>
        public string SelectWeChat { get; set; }
        /// <summary>
        /// 当前选中的操作集合
        /// </summary>
        public string SelectOperate { get; set; }
        /// <summary>
        /// 当前选中的绑定还是订阅
        /// </summary>
        public string SelectBindOrSub { get; set; }
        /// <summary>
        /// 当前选中的微信客户
        /// </summary>
        public string SelectCompany { get; set; }
        /// <summary>
        /// 当前选中的消息类型
        /// </summary>
        public string SelectMsgType { get; set; }
        /// <summary>
        /// 当前选中要发送的用户
        /// </summary>
        public string SelectUser { get; set; }
        /// <summary>
        /// 文本消息
        /// </summary>
        public WeChatPushTextContentDto TextContent { get; set; }
        /// <summary>
        /// 图片消息
        /// </summary>
        public WeChatPushPictureContentDto PictureContent { get; set; }
        /// <summary>
        /// 语音消息
        /// </summary>
        public WeChatPushVoiceContentDto VoiceContent { get; set; }
        /// <summary>
        /// 视频消息
        /// </summary>
        public WeChatPushVideoContentDto VideoContent { get; set; }
        /// <summary>
        /// 链接消息
        /// </summary>
        public WeChatPushLinkMsgContentDto LinkMsgContent { get; set; }


    }
}
