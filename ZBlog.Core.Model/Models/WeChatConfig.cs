using SqlSugar;

namespace ZBlog.Core.Model.Models
{
    ///<summary>
    /// 微信配置
    ///</summary>
    [SugarTable("WeChatConfig")]
    public class WeChatConfig
    {

        /// <summary>
        /// 微信公众号唯一标识
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, Length = 100, IsNullable = false)]
        public string PublicAccount { get; set; }

        /// <summary>
        /// 微信公众号名称
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = false)]
        public string PublicNick { get; set; }

        /// <summary>
        /// 微信账号
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = false)]
        public string WeChatAccount { get; set; }

        /// <summary>
        /// 微信名称
        /// </summary>
        [SugarColumn(Length = 200)]
        public string WeChatNick { get; set; }

        /// <summary>
        /// 应用ID
        /// </summary>
        [SugarColumn(Length = 100)]
        public string Appid { get; set; }

        /// <summary>
        /// 应用秘钥
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = false)]
        public string Appsecret { get; set; }

        /// <summary>
        /// 公众号推送token
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true)]
        public string Token { get; set; }

        /// <summary>
        /// 验证秘钥(验证消息是否真实)
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = false)]
        public string InteractiveToken { get; set; }

        /// <summary>
        /// 微信公众号token过期时间
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public DateTime? TokenExpiration { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true)]
        public string Remark { get; set; }

        /// <summary>
        /// 是否激活
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// 创建者id
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public int? CreateId { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public string CreateBy { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// 修改者id
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public int? ModifyId { get; set; }

        /// <summary>
        /// 修改人
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public string ModifyBy { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public DateTime? ModifyTime { get; set; }
    }
}
