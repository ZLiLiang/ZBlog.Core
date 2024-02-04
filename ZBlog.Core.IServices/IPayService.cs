using ZBlog.Core.IServices.Base;
using ZBlog.Core.Model;
using ZBlog.Core.Model.Models.RootTkey;
using ZBlog.Core.Model.ViewModels;

namespace ZBlog.Core.IServices
{
    /// <summary>
    /// IPayService
    /// </summary>	
    public interface IPayService : IBaseService<RootEntityTkey<int>>
    {
        /// <summary>
        /// 被扫支付
        /// </summary>
        /// <returns></returns>
        Task<MessageModel<PayReturnResultModel>> Pay(PayNeedModel payModel);
        /// <summary>
        /// 退款
        /// </summary>
        /// <param name="payModel"></param>
        /// <returns></returns>
        Task<MessageModel<PayRefundReturnResultModel>> PayRefund(PayRefundNeedModel payModel);
        /// <summary>
        /// 轮询查询
        /// </summary>
        /// <param name="payModel"></param>
        /// <param name="times">轮询次数</param>
        /// <returns></returns>
        Task<MessageModel<PayReturnResultModel>> PayCheck(PayNeedModel payModel, int times);
        /// <summary>
        /// 验证签名
        /// </summary>
        /// <param name="strSrc">参数</param>
        /// <param name="sign">签名</param>
        /// <param name="pubKey">公钥</param>
        /// <returns></returns>
        bool NotifyCheck(string strSrc, string sign, string pubKey);

    }
}
