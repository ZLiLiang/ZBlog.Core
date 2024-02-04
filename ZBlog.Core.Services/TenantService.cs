using ZBlog.Core.Common.DataBase;
using ZBlog.Core.Common.Seed;
using ZBlog.Core.IServices;
using ZBlog.Core.Model.Models;
using ZBlog.Core.Repository.UnitOfWorks;
using ZBlog.Core.Services.Base;

namespace ZBlog.Core.Services
{
    public class TenantService : BaseService<SysTenant>, ITenantService
    {
        private readonly IUnitOfWorkManage _uowManage;

        public TenantService(IUnitOfWorkManage uowManage)
        {
            _uowManage = uowManage;
        }

        public async Task SaveTenant(SysTenant tenant)
        {
            bool initDb = tenant.Id == 0;
            using (var uow = _uowManage.CreateUnitOfWork())
            {
                tenant.DefaultTenantConfig();

                if (tenant.Id == 0)
                {
                    await Db.Insertable(tenant)
                        .ExecuteReturnSnowflakeIdAsync();
                }
                else
                {
                    var oldTenant = await base.QueryById(tenant.Id);
                    if (oldTenant.Connection != tenant.Connection)
                        initDb = true;
                    await Db.Updateable(tenant)
                        .ExecuteCommandAsync();
                }

                uow.Commit();
            }

            if (initDb)
                await InitTenantDb(tenant);
        }

        public async Task InitTenantDb(SysTenant tenant)
        {
            await DBSeed.InitTenantSeedAsync(Db.AsTenant(), tenant.GetConnectionConfig());
        }
    }
}
