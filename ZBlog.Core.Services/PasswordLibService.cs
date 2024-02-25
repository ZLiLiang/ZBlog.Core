using ZBlog.Core.Common.CustomAttribute;
using ZBlog.Core.Common.DB;
using ZBlog.Core.IServices;
using ZBlog.Core.Model.Models;
using ZBlog.Core.Repository.Base;
using ZBlog.Core.Services.Base;

namespace ZBlog.Core.Services
{
    public partial class PasswordLibService : BaseService<PasswordLib>, IPasswordLibService
    {
        IBaseRepository<PasswordLib> _dal;

        public PasswordLibService(IBaseRepository<PasswordLib> dal)
        {
            this._dal = dal;
            base.BaseDal = dal;
        }

        [UseTran(Propagation = Propagation.Required)]
        public async Task<bool> TestTranPropagation2()
        {
            await _dal.Add(new PasswordLib
            {
                IsDeleted = false,
                AccountName = "aaa",
                CreateTime = DateTime.Now
            });

            return true;
        }

        [UseTran(Propagation = Propagation.Mandatory)]
        public async Task<bool> TestTranPropagationNoTranError()
        {
            await _dal.Add(new PasswordLib
            {
                IsDeleted = false,
                AccountName = "aaa",
                CreateTime = DateTime.Now
            });

            return true;
        }

        [UseTran(Propagation = Propagation.Nested)]
        public async Task<bool> TestTranPropagationTran2()
        {
            await _dal.Add(new PasswordLib
            {
                IsDeleted = false,
                AccountName = "aaa",
                CreateTime = DateTime.Now
            });

            return true;
        }
    }
}
