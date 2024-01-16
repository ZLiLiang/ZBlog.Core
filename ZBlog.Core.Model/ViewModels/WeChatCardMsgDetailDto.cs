namespace ZBlog.Core.Model.ViewModels
{
    /// <summary>
    /// 消息模板dto(如何填写数据,请参考微信模板即可)
    /// 作者:胡丁文
    /// 时间:2020-4-1 09:32:16
    /// </summary>
    public class WeChatCardMsgDetailDto
    {
        /// <summary>
        /// 消息模板
        /// </summary>
        public string Template_Id { get; set; }
        /// <summary>
        /// 标题
        /// </summary>
        public string First { get; set; }
        /// <summary>
        /// 标题颜色(颜色代码都必须为#开头的16进制代码)
        /// </summary>
        public string ColorFirst { get; set; } = "#173177";
        /// <summary>
        /// 内容1
        /// </summary>
        public string Keyword1 { get; set; }
        /// <summary>
        /// 内容1颜色
        /// </summary>

        public string Color1 { get; set; } = "#173177";
        /// <summary>
        /// 内容2
        /// </summary>
        public string Keyword2 { get; set; }
        /// <summary>
        /// 内容2颜色
        /// </summary>
        public string Color2 { get; set; } = "#173177";
        /// <summary>
        /// 内容3
        /// </summary>
        public string Keyword3 { get; set; }
        /// <summary>
        /// 内容3颜色
        /// </summary>
        public string Color3 { get; set; } = "#173177";
        /// <summary>
        /// 内容4
        /// </summary>
        public string Keyword4 { get; set; }
        /// <summary>
        /// 内容4颜色
        /// </summary>
        public string Color4 { get; set; } = "#173177";
        /// <summary>
        /// 内容5
        /// </summary>
        public string Keyword5 { get; set; }
        /// <summary>
        /// 内容5颜色
        /// </summary>
        public string Color5 { get; set; } = "#173177";
        /// <summary>
        /// 备注信息
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 备注信息颜色
        /// </summary>
        public string ColorRemark { get; set; } = "#173177";
        /// <summary>
        /// 跳转连接
        /// </summary>
        public string Url { get; set; }
    }
}
