using SqlSugar;

namespace ZBlog.Core.Model.Models
{
    /// <summary>
    /// 微信订阅
    /// </summary>
    [SugarTable("WeChatSub")]
    public partial class WeChatSub
    {
        [SugarColumn(IsNullable = false, IsPrimaryKey = true)]
        public string Id { get; set; }

        /// <summary>
        /// 来自哪个公众号
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = false, IndexGroupNameList = new string[] { "index" })]
        public string SubFromPublicAccount { get; set; }

        /// <summary>
        /// 绑定公司id
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = false, IndexGroupNameList = new string[] { "index" })]
        public string CompanyId { get; set; }

        /// <summary>
        /// 绑定员工id
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = false, IndexGroupNameList = new string[] { "index" })]
        public string SubJobId { get; set; }

        /// <summary>
        /// 绑定微信id
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = false)]
        public string SubUserOpenId { get; set; }

        /// <summary>
        /// 绑定微信联合id
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true)]
        public string SubUserUnionId { get; set; }

        /// <summary>
        /// 绑定时间
        /// </summary>
        public DateTime SubUserRegTime { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public DateTime? SubUserRefTime { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true)]
        public string SubUserRemark { get; set; }

        /// <summary>
        /// 是否已解绑
        /// </summary>
        public bool IsUnBind { get; set; }

        /// <summary>
        /// 上次绑定微信id
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true)]
        public string LastSubUserOpenId { get; set; }

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
