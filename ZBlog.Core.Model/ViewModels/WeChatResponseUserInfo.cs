namespace ZBlog.Core.Model.ViewModels
{
    /// <summary>
    /// 返回给调用者的Dto
    /// </summary>
    public class WeChatResponseUserInfo
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
        /// 数据
        /// </summary>
        public WeChatApiDto UsersData { get; set; }
    }
}
