using SqlSugar;
using ZBlog.Core.Model.Models.RootTkey;
using ZBlog.Core.Model.Tenants;

namespace ZBlog.Core.Model.Models.Tenant
{
    /// <summary>
    /// 多租户-多表方案 业务表 <br/>
    /// </summary>
    [MultiTenant(TenantTypeEnum.Tables)]
    public class MultiBusinessTable : BaseEntity
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 金额
        /// </summary>
        public decimal Amount { get; set; }

        [Navigate(NavigateType.OneToMany, nameof(MultiBusinessSubTable.MainId))]
        public List<MultiBusinessSubTable> SubTables { get; set; }
    }
}
