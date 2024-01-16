namespace ZBlog.Core.Model.ViewModels
{
    /// <summary>
    /// 微信接口消息DTO
    /// </summary>
    public class WeChatApiDto
    {
        /// <summary>
        /// 微信公众号ID(数据库查询)
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 错误代码
        /// </summary>
        public int Errcode { get; set; }
        /// <summary>
        /// 错误信息
        /// </summary>
        public string Errmsg { get; set; }


        /// <summary>
        /// token
        /// </summary>
        public string Access_Token { get; set; }
        /// <summary>
        /// 过期时间(秒)
        /// </summary>
        public int Expires_In { get; set; }


        /// <summary>
        /// 用户关注数
        /// </summary>
        public int Total { get; set; }
        /// <summary>
        /// 获取用户数量
        /// </summary>
        public int Count { get; set; }
        /// <summary>
        /// 获取用户OpenIDs
        /// </summary>
        public WeChatOpenIDsDto Data { get; set; }
        public List<WeChatApiDto> Users { get; set; }
        /// <summary>
        /// 下一个关注用户
        /// </summary>
        public string Next_Openid { get; set; }

        /// <summary>
        /// 微信消息模板列表
        /// </summary>

        public WeChatTemplateList[] Template_List { get; set; }
        /// <summary>
        /// 微信菜单
        /// </summary>
        public WeChatMenuDto Menu { get; set; }

        /// <summary>
        /// 二维码票据
        /// </summary>
        public string Ticket { get; set; }
        /// <summary>
        /// 二维码过期时间
        /// </summary>
        public int Expire_Seconds { get; set; }
        /// <summary>
        /// 二维码地址
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// 关注状态
        /// </summary> 
        public string Subscribe { get; set; }
        /// <summary>
        /// 用户微信ID
        /// </summary>
        public string Openid { get; set; }
        /// <summary>
        /// 昵称
        /// </summary>
        public string Nickname { get; set; }
        /// <summary>
        /// 性别
        /// </summary>
        public int Sex { get; set; }
        /// <summary>
        /// 语言
        /// </summary>
        public string Language { get; set; }
        /// <summary>
        /// 城市
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// 省份
        /// </summary>
        public string Province { get; set; }
        /// <summary>
        /// 城市
        /// </summary>
        public string Country { get; set; }
        /// <summary>
        /// 头像地址
        /// </summary>
        public string Headimgurl { get; set; }
    }
}
