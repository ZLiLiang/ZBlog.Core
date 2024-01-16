namespace ZBlog.Core.Model.ViewModels
{
    /// <summary>
    /// 微信二维码预装信息DTO
    /// </summary>
    public class WeChatQRDto
    {
        public int Expire_Seconds { get; set; }
        public string Action_Name { get; set; }
        public WeChatQRActionDto Action_Info { get; set; }
    }
}
