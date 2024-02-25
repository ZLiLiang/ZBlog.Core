using ZBlog.Core.Common.DB.Extension;
using ZBlog.Core.IServices.IDS4Db;
using ZBlog.Core.Model.IDS4DbModels;
using ZBlog.Core.Services.Base;

namespace ZBlog.Core.Services.IDS4Db
{
    public class ApplicationUserService : BaseService<ApplicationUser>, IApplicationUserService
    {
        public bool IsEnable()
        {
            var configId = typeof(ApplicationUser).GetEntityTenant();

            return Db.AsTenant().IsAnyConnection(configId);
        }
    }
}
