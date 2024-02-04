using ZBlog.Core.IServices.Base;
using ZBlog.Core.Model.Models;

namespace ZBlog.Core.IServices
{
    public interface ITenantService : IBaseService<SysTenant>
    {
        Task SaveTenant(SysTenant tenant);

        Task InitTenantDb(SysTenant tenant);
    }
}
