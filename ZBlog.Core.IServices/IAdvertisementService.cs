using ZBlog.Core.IServices.Base;
using ZBlog.Core.Model.Models;

namespace ZBlog.Core.IServices
{
    public interface IAdvertisementService : IBaseService<Advertisement>
    {
        void ReturnExp();
    }
}
