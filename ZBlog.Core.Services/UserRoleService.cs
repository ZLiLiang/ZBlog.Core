using SqlSugar.Extensions;
using ZBlog.Core.Common.CustomAttribute;
using ZBlog.Core.IServices;
using ZBlog.Core.Model.Models;
using ZBlog.Core.Services.Base;

namespace ZBlog.Core.Services
{
    /// <summary>
    /// UserRoleService
    /// </summary>
    public class UserRoleService : BaseService<UserRole>, IUserRoleService
    {
        public async Task<UserRole> SaveUserRole(long uid, long rid)
        {
            UserRole userRole = new UserRole(uid, rid);
            var userList = await base.Query(it => it.UserId == userRole.UserId && it.RoleId == userRole.RoleId);
            if (userList.Count > 0)
            {
                return userList.FirstOrDefault();
            }
            else
            {
                var id = await base.Add(userRole);
                return await base.QueryById(id);
            }
        }

        [Caching(AbsoluteExpiration = 30)]
        public async Task<int> GetRoleIdByUid(long uid)
        {
            return ((await base.Query(it => it.UserId == uid))
                 .OrderByDescending(it => it.Id)
                 .LastOrDefault()?
                 .RoleId)
                 .ObjToInt();
        }
    }
}
