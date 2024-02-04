using ZBlog.Core.Common.CustomAttribute;
using ZBlog.Core.IServices;
using ZBlog.Core.Model.Models;
using ZBlog.Core.Services.Base;

namespace ZBlog.Core.Services
{
    /// <summary>
    /// RoleService
    /// </summary>	
    public class RoleService : BaseService<Role>, IRoleService
    {
        public async Task<Role> SaveRole(string roleName)
        {
            var role = new Role(roleName);
            var userList = await base.Query(it => it.Name == role.Name && it.Enabled);

            if (userList.Count > 0)
            {
                return userList.FirstOrDefault();
            }
            else
            {
                var id = await base.Add(role);
                return await base.QueryById(id);
            }
        }

        [Caching(AbsoluteExpiration = 30)]
        public async Task<string> GetRoleNameByRid(int rid)
        {
            return (await base.QueryById(rid))?.Name;
        }
    }
}
