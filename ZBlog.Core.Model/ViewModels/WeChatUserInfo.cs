namespace ZBlog.Core.Model.ViewModels
{
    /// <summary>
    /// 微信推送所需信息(公司版本)
    /// </summary>
    public class WeChatUserInfo
    {
        /// <summary>
        /// 微信公众号ID
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 公司代码
        /// </summary>
        public string CompanyCode { get; set; }
        /// <summary>
        /// 用户id
        /// </summary>
        public string UserID { get; set; }
        /// <summary>
        /// 用户昵称
        /// </summary>
        public string UserNick { get; set; }
    }
}
