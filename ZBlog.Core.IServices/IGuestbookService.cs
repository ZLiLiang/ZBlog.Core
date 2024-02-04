using ZBlog.Core.IServices.Base;
using ZBlog.Core.Model.Models;
using ZBlog.Core.Model;

namespace ZBlog.Core.IServices
{
    public partial interface IGuestbookService : IBaseService<Guestbook>
    {
        Task<MessageModel<string>> TestTranInRepository();

        Task<bool> TestTranInRepositoryAOP();

        Task<bool> TestTranPropagation();

        Task<bool> TestTranPropagationNoTran();

        Task<bool> TestTranPropagationTran();
    }
}
