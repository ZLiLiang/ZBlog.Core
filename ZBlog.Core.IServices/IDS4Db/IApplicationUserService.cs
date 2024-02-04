using ZBlog.Core.IServices.Base;
using ZBlog.Core.Model.IDS4DbModels;

namespace ZBlog.Core.IServices.IDS4Db
{
    public interface IApplicationUserService:IBaseService<ApplicationUser>
    {
        bool IsEnable();
    }
}
