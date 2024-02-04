using Microsoft.Extensions.Logging;
using ZBlog.Core.IServices;
using ZBlog.Core.Model.Models;
using ZBlog.Core.Repository.UnitOfWorks;
using ZBlog.Core.Services.Base;

namespace ZBlog.Core.Services
{
    public class WeChatCompanyService : BaseService<WeChatCompany>, IWeChatCompanyService
    {
        private readonly IUnitOfWorkManage _unitOfWorkManage;
        private ILogger<WeChatCompanyService> _logger;

        public WeChatCompanyService(IUnitOfWorkManage unitOfWorkManage, ILogger<WeChatCompanyService> logger)
        {
            _unitOfWorkManage = unitOfWorkManage;
            _logger = logger;
        }
    }
}
