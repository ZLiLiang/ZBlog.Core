using ZBlog.Core.IServices.Base;
using ZBlog.Core.Model.Models;

namespace ZBlog.Core.IServices
{
    /// <summary>
	/// sysUserInfoService
	/// </summary>	
    public interface ISysUserInfoService : IBaseService<SysUserInfo>
    {
        Task<SysUserInfo> SaveUserInfo(string loginName, string loginPwd);
        Task<string> GetUserRoleNameStr(string loginName, string loginPwd);
    }
}
