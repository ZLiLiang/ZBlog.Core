namespace ZBlog.Core.Model.ViewModels
{
    /// <summary>
    /// 获取微信菜单DTO,用于存放具体菜单内容
    /// </summary>
    public class WeChatMenuButtonDto
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string Key { get; set; }
        public string Url { get; set; }
        public WeChatMenuButtonDto[] Sub_Button { get; set; }
    }
}
