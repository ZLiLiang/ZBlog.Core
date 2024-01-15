using ZBlog.Core.Model.Models.RootTkey;
using ZBlog.Core.Model.Tenants;

namespace ZBlog.Core.Model.Models.Tenant
{
    /// <summary>
    /// 多租户-多表方案 业务表 子表 <br/>
    /// </summary>
    [MultiTenant(TenantTypeEnum.Tables)]
    public class MultiBusinessSubTable:BaseEntity
    {
        public long MainId { get; set; }

        public string Memo { get; set; }
    }
}
