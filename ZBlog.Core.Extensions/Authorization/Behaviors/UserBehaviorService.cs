using Microsoft.Extensions.Logging;
using ZBlog.Core.Common.Helper;
using ZBlog.Core.Common.HttpContextUser;
using ZBlog.Core.IServices;

namespace ZBlog.Core.Extensions.Authorizations.Behaviors
{
    public class UserBehaviorService : IUserBehaviorService
    {
        private readonly IUser _user;
        private readonly ISysUserInfoService _sysUserInfoService;
        private readonly ILogger<UserBehaviorService> _logger;
        private readonly string _uid;
        private readonly string _token;

        public UserBehaviorService(IUser user, ISysUserInfoService sysUserInfoService, ILogger<UserBehaviorService> logger)
        {
            _user = user;
            _sysUserInfoService = sysUserInfoService;
            _logger = logger;
            _uid = _user.ID.ObjToString();
            _token = _user.GetToken();
        }

        public Task<bool> CreateOrUpdateUserAccessByUid()
        {
            throw new NotImplementedException();
        }

        public Task<bool> RemoveAllUserAccessByUid()
        {
            throw new NotImplementedException();
        }

        public Task<bool> CheckUserIsNormal()
        {
            throw new NotImplementedException();
        }

        public Task<bool> CheckTokenIsNormal()
        {
            throw new NotImplementedException();
        }
    }
}
