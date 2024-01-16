namespace ZBlog.Core.Model.ViewModels
{
    /// <summary>
    /// 推送详细数据
    /// </summary>
    public class WeChatPushCardMsgDetailDto
    {
        public WeChatPushCardMsgValueColorDto First { get; set; }
        public WeChatPushCardMsgValueColorDto Keyword1 { get; set; }
        public WeChatPushCardMsgValueColorDto Keyword2 { get; set; }
        public WeChatPushCardMsgValueColorDto Keyword3 { get; set; }
        public WeChatPushCardMsgValueColorDto Keyword4 { get; set; }
        public WeChatPushCardMsgValueColorDto Keyword5 { get; set; }
        public WeChatPushCardMsgValueColorDto Remark { get; set; }
    }
}
