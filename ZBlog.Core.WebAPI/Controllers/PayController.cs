using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZBlog.Core.Common.GlobalVars;
using ZBlog.Core.IServices;
using ZBlog.Core.Model.ViewModels;
using ZBlog.Core.Model;

namespace ZBlog.Core.WebAPI.Controllers
{
    /// <summary>
    /// 建行聚合支付类
    /// </summary>
    [Produces("application/json")]
    [Route("api/Pay")]
    [Authorize(Permissions.Name)]
    public class PayController : Controller
    {
        private readonly ILogger<PayController> _logger;
        private readonly IPayService _payService;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="payService"></param>
        public PayController(ILogger<PayController> logger, IPayService payService)
        {
            _logger = logger;
            _payService = payService;
        }

        /// <summary>
        /// 被扫支付
        /// </summary>
        /// <param name="payModel"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Pay")]
        public async Task<MessageModel<PayReturnResultModel>> PayGet([FromQuery] PayNeedModel payModel)
        {
            return await _payService.Pay(payModel);
        }

        /// <summary>
        /// 被扫支付
        /// </summary>
        /// <param name="payModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Pay")]
        public async Task<MessageModel<PayReturnResultModel>> PayPost([FromBody] PayNeedModel payModel)
        {
            return await _payService.Pay(payModel);
        }

        /// <summary>
        /// 支付结果查询-轮询
        /// </summary>
        /// <param name="payModel"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("PayCheck")]
        public async Task<MessageModel<PayReturnResultModel>> PayCheckGet([FromQuery] PayNeedModel payModel)
        {
            return await _payService.PayCheck(payModel, 1);
        }

        /// <summary>
        /// 支付结果查询-轮询
        /// </summary>
        /// <param name="payModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("PayCheck")]
        public async Task<MessageModel<PayReturnResultModel>> PayCheckPost([FromBody] PayNeedModel payModel)
        {
            return await _payService.PayCheck(payModel, 1);
        }

        /// <summary>
        /// 退款
        /// </summary>
        /// <param name="payModel"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("PayRefund")]
        public async Task<MessageModel<PayRefundReturnResultModel>> PayRefundGet([FromQuery] PayRefundNeedModel payModel)
        {
            return await _payService.PayRefund(payModel);
        }

        /// <summary>
        /// 退款
        /// </summary>
        /// <param name="payModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("PayRefund")]
        public async Task<MessageModel<PayRefundReturnResultModel>> PayRefundPost([FromBody] PayRefundNeedModel payModel)
        {
            return await _payService.PayRefund(payModel);
        }
    }
}
