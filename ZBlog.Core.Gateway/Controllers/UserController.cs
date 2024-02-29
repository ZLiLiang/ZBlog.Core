using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZBlog.Core.Common.GlobalVars;
using ZBlog.Core.Common.HttpContextUser;
using ZBlog.Core.Model;

namespace ZBlog.Core.Gateway.Controllers
{
    [Authorize(AuthenticationSchemes = Permissions.GWName)]
    [Route("/gateway/[controller]/[action]")]
    public class UserController : ControllerBase
    {
        private readonly IUser _user;

        public UserController(IUser user)
        {
            _user = user;
        }

        [HttpGet]
        public MessageModel<List<ClaimDto>> MyClaims()
        {
            return new MessageModel<List<ClaimDto>>()
            {
                Success = true,
                Response = _user.GetClaimsIdentity()
                    .Select(d => new ClaimDto
                    {
                        Type = d.Type,
                        Value = d.Value
                    })
                    .ToList()
            };
        }
    }

    public class ClaimDto
    {
        public string Type { get; set; }
        public string Value { get; set; }
    }
}
