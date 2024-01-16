using SqlSugar;
using ZBlog.Core.Model.Models.RootTkey;

namespace ZBlog.Core.Model.Models
{
    /// <summary>
    /// 密码库表
    /// </summary>
    [SugarTable("PasswordLib", "密码库表")] //('数据库表名'，'数据库表备注')
    public class PasswordLib
    {
        [SugarColumn(IsNullable = false, IsPrimaryKey = true, IsIdentity = false)]
        public long Id { get; set; }

        /// <summary>
        /// 获取或设置是否禁用，逻辑上的删除，非物理删除
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public bool? IsDeleted { get; set; }

        [SugarColumn(Length = 200, IsNullable = true)]
        public string Url { get; set; }

        [SugarColumn(Length = 100, IsNullable = true)]
        public string Pwd { get; set; }

        [SugarColumn(Length = 200, IsNullable = true)]
        public string AccountName { get; set; }

        [SugarColumn(IsNullable = true)]
        public int? Status { get; set; }

        [SugarColumn(IsNullable = true)]
        public int? ErrorCount { get; set; }

        [SugarColumn(Length = 200, IsNullable = true)]
        public string HintPwd { get; set; }

        [SugarColumn(Length = 200, IsNullable = true)]
        public string Hintquestion { get; set; }

        [SugarColumn(IsNullable = true)]
        public DateTime? CreateTime { get; set; }

        [SugarColumn(IsNullable = true)]
        public DateTime? UpdateTime { get; set; }

        [SugarColumn(IsNullable = true)]
        public DateTime? LastErrTime { get; set; }

        [SugarColumn(Length = 200, IsNullable = true)]
        public string Test { get; set; }
    }
}
