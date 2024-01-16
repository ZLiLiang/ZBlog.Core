namespace ZBlog.Core.Model.ViewModels
{
    /// <summary>
    /// 微信验证Dto
    /// </summary> 
    public class WeChatValidDto
    {
        /// <summary>
        /// 微信公众号唯一标识
        /// </summary>
        public string PublicAccount { get; set; }
        /// <summary>
        /// 验证成功后返回给微信的字符串
        /// </summary>
        public string EchoStr { get; set; }
        /// <summary>
        /// 签名
        /// </summary>
        public string Signature { get; set; }
        /// <summary>
        /// 时间戳
        /// </summary>
        public string Timestamp { get; set; }
        /// <summary>
        /// 随机数
        /// </summary>
        public string Nonce { get; set; }

    }
}
