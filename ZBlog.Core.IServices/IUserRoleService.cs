using ZBlog.Core.IServices.Base;
using ZBlog.Core.Model.Models;

namespace ZBlog.Core.IServices
{
    /// <summary>
	/// UserRoleService
	/// </summary>	
    public interface IUserRoleService : IBaseService<UserRole>
    {

        Task<UserRole> SaveUserRole(long uid, long rid);
        Task<int> GetRoleIdByUid(long uid);
    }
}
