using ZBlog.Core.IServices.Base;
using ZBlog.Core.Model.Models;

namespace ZBlog.Core.IServices
{
    /// <summary>
	/// RoleService
	/// </summary>	
    public interface IRoleService : IBaseService<Role>
    {
        Task<Role> SaveRole(string roleName);
        Task<string> GetRoleNameByRid(int rid);

    }
}
