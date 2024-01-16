namespace ZBlog.Core.Model.ViewModels
{
    /// <summary>
    /// 微信推送所需信息(OpenID版本)
    /// </summary>
    public class WeChatUserInfoOpenID
    {
        /// <summary>
        /// 微信公众号ID
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 微信OpenID
        /// </summary>
        public List<string> UserID { get; set; }
    }
}
