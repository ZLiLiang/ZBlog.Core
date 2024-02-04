using ZBlog.Core.Common.Helper;
using ZBlog.Core.IServices;
using ZBlog.Core.Model.Models;
using ZBlog.Core.Repository.Base;
using ZBlog.Core.Services.Base;

namespace ZBlog.Core.Services
{
    public class SysUserInfoService : BaseService<SysUserInfo>, ISysUserInfoService
    {
        private readonly IBaseRepository<UserRole> _userRoleRepository;
        private readonly IBaseRepository<Role> _roleRepository;

        public SysUserInfoService(IBaseRepository<UserRole> userRoleRepository, IBaseRepository<Role> roleRepository)
        {
            _userRoleRepository = userRoleRepository;
            _roleRepository = roleRepository;
        }

        public async Task<SysUserInfo> SaveUserInfo(string loginName, string loginPwd)
        {
            SysUserInfo userInfo = new SysUserInfo(loginName, loginPwd);
            var userList = await base.Query(it => it.LoginName == userInfo.LoginName && it.LoginPWD == userInfo.LoginPWD);
            if (userList.Count > 0)
            {
                return userList.FirstOrDefault();
            }
            else
            {
                var id = await base.Add(userInfo);
                return await base.QueryById(id);
            }
        }

        public async Task<string> GetUserRoleNameStr(string loginName, string loginPwd)
        {
            string roleName = "";
            var user = (await base.Query(it => it.LoginName == loginName && it.LoginPWD == loginPwd))
                .FirstOrDefault();
            var roleList = await _roleRepository.Query(it => it.IsDeleted == false);

            if (user != null)
            {
                var userRoles = await _userRoleRepository.Query(it => it.UserId == user.Id);
                if (userRoles.Count > 0)
                {
                    var arr = userRoles.Select(it => it.RoleId.ObjToString())
                        .ToList();
                    var roles = roleList.Where(it => arr.Contains(it.Id.ObjToString()));

                    roleName = string.Join(',', roles.Select(it => it.Name).ToArray());
                }
            }

            return roleName;
        }
    }
}
