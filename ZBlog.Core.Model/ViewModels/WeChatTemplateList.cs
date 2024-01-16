namespace ZBlog.Core.Model.ViewModels
{
    /// <summary>
    /// 微信消息模板Dto
    /// </summary>
    public class WeChatTemplateList
    {
        public string Template_Id { get; set; }
        public string Title { get; set; }
        public string Primary_Industry { get; set; }
        public string Deputy_Industry { get; set; }
        public string Content { get; set; }
        public string Example { get; set; }
    }
}
