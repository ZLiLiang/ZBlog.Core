using ZBlog.Core.IServices;
using ZBlog.Core.Model.Models;
using ZBlog.Core.Repository.Base;
using ZBlog.Core.Services.Base;

namespace ZBlog.Core.Services
{
    /// <summary>
    /// sysUserInfoService
    /// </summary>	
    public class SplitDemoService : BaseService<SplitDemo>, ISplitDemoService
    {
        private readonly IBaseRepository<SplitDemo> _splitDemoRepository;
        public SplitDemoService(IBaseRepository<SplitDemo> splitDemoRepository)
        {
            _splitDemoRepository = splitDemoRepository;
        }


    }
}
