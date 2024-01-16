namespace ZBlog.Core.Model.ViewModels
{
    /// <summary>
    /// 推送给微信所需Dto
    /// </summary>
    public class WeChatPushCardMsgDto
    {
        /// <summary>
        /// 推送微信用户ID
        /// </summary>
        public string Touser { get; set; }
        /// <summary>
        /// 推送的模板ID
        /// </summary>
        public string Template_Id { get; set; }
        /// <summary>
        /// 推送URL地址
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// 推送的数据
        /// </summary>
        public WeChatPushCardMsgDetailDto Data { get; set; }
    }
}
