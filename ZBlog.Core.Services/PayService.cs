using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ZBlog.Core.Common.Helper;
using ZBlog.Core.Common.Static;
using ZBlog.Core.IServices;
using ZBlog.Core.Model;
using ZBlog.Core.Model.Models.RootTkey;
using ZBlog.Core.Model.ViewModels;
using ZBlog.Core.Services.Base;

namespace ZBlog.Core.Services
{
    public partial class PayService : BaseService<RootEntityTkey<int>>, IPayService
    {
        IHttpContextAccessor _httpContextAccessor;
        ILogger<PayService> _logger;

        public PayService(IHttpContextAccessor httpContextAccessor, ILogger<PayService> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<MessageModel<PayReturnResultModel>> Pay(PayNeedModel payModel)
        {
            _logger.LogInformation("支付开始");
            MessageModel<PayReturnResultModel> messageModel = new MessageModel<PayReturnResultModel>
            {
                Response = new PayReturnResultModel()
            };
            string url = string.Empty;
            string param = string.Empty;
            string returnData = string.Empty;

            try
            {
                _logger.LogInformation($"原始GET参数->{_httpContextAccessor.HttpContext.Request.QueryString}");
                //被扫支付 
                string host = "https://ibsbjstar.ccb.com.cn/CCBIS/B2CMainPlat_00_BEPAY?";
                ////商户信息
                //string merInfo = "MERCHANTID=105910100190000&POSID=000000000&BRANCHID=610000000";
                ////获取柜台完整公钥
                //string pubKey = "30819d300d06092a864886f70d010101050003818b0030818702818100a32fb2d51dda418f65ca456431bd2f4173e41a82bb75c2338a6f649f8e9216204838d42e2a028c79cee19144a72b5b46fe6a498367bf4143f959e4f73c9c4f499f68831f8663d6b946ae9fa31c74c9332bebf3cba1a98481533a37ffad944823bd46c305ec560648f1b6bcc64d54d32e213926b26cd10d342f2c61ff5ac2d78b020111";
                ////加密原串
                //string param = merInfo + "&MERFLAG=1&TERMNO1=&TERMNO2=&ORDERID=937857156" +
                //        "&QRCODE=134737690209713400&AMOUNT=0.01&TXCODE=PAY100&PROINFO=&REMARK1=&REMARK2=&SMERID=&SMERNAME=&SMERTYPEID=" +
                //        "&SMERTYPE=&TRADECODE=&TRADENAME=&SMEPROTYPE=&PRONAME=";

                Dictionary<string, object> dic = new Dictionary<string, object>();

                //支付信息
                dic.Add("MERCHANTID", StaticPayInfo.MERCHANTID);// => self::MERCHANTID, // 商户号
                dic.Add("POSID", StaticPayInfo.POSID);// => self::POSID, // 柜台号
                dic.Add("BRANCHID", StaticPayInfo.BRANCHID);// => self::BRANCHID, // 分行号 
                dic.Add("TXCODE", "PAY100");// => 'PAY100', // 交易码
                dic.Add("MERFLAG", "1");// => '', // 商户类型 1线上 2线下 
                dic.Add("ORDERID", payModel.ORDERID);//payModel.ORDERID);// => '', // 订单号
                dic.Add("QRCODE", payModel.QRCODE);// => '', // 码信息（一维码、二维码）
                dic.Add("AMOUNT", payModel.AMOUNT);// => '0.01', // 订单金额，单位：元
                dic.Add("PROINFO", payModel.PROINFO);// => '', // 商品名称
                dic.Add("REMARK1", payModel.REMARK1);// => '', // 备注 1
                dic.Add("REMARK2", payModel.REMARK2);// => '', // 备注 2

                //dic.Add("TERMNO1", "");// => '', // 终端编号 1
                //dic.Add("TERMNO2", "");// => '', // 终端编号 2
                //dic.Add("GROUPMCH", "");// => '', // 集团商户信息
                //dic.Add("FZINFO1", "");// => '', // 分账信息一
                //dic.Add("FZINFO2", "");// => '', // 分账信息二
                //dic.Add("SUB_APPID", "");// => '', // 子商户公众账号 ID
                //dic.Add("RETURN_FIELD", "");// => '', // 返回信息位图
                //dic.Add("USERPARAM", "");// => '', // 实名支付
                //dic.Add("detail", "");// => '', // 商品详情
                //dic.Add("goods_tag", "");// => '', // 订单优惠标记 

                //商户信息
                Dictionary<string, object> dicInfo = new Dictionary<string, object>();
                dicInfo.Add("MERCHANTID", StaticPayInfo.MERCHANTID);// => self::MERCHANTID, // 商户号
                dicInfo.Add("POSID", StaticPayInfo.POSID);// => self::POSID, // 柜台号
                dicInfo.Add("BRANCHID", StaticPayInfo.BRANCHID);// => self::BRANCHID, // 分行号
                var Info = StringHelper.GetPars(dicInfo);

                //获取拼接请求串
                param = StringHelper.GetPars(dic);

                //加密
                var paramEncryption = new CCBPayUtil().makeCCBParam(param, StaticPayInfo.pubKey);
                //拼接请求串
                url = host + Info + "&ccbParam=" + paramEncryption;
                //请求 
                _logger.LogInformation($"请求地址->{url}");
                _logger.LogInformation($"请求参数->{param}");
                PayResultModel payResult;

                try
                {
                    returnData = await HttpHelper.PostAsync(url);

                    //转换数据
                    try
                    {
                        payResult = JsonHelper.ParseFormByJson<PayResultModel>(returnData);
                    }
                    catch
                    {
                        payResult = new PayResultModel { RESULT = "N", ERRMSG = "参数错误", ORDERID = payModel.ORDERID, AMOUNT = payModel.AMOUNT };
                        returnData = StringHelper.GetCusLine(returnData, 15);
                    }
                    _logger.LogInformation($"响应数据->{returnData}");
                }
                catch (Exception ex)
                {
                    _logger.LogInformation($"异常信息:{ex.Message}");
                    _logger.LogInformation($"异常堆栈:{ex.StackTrace}");
                    messageModel = await PayCheck(payModel, 1);
                    return messageModel;
                }

                switch (payResult.RESULT)
                {
                    case "Y":
                        Dictionary<string, object> dicCheckPars = new Dictionary<string, object>();
                        dicCheckPars.Add("RESULT", payResult.RESULT);
                        dicCheckPars.Add("ORDERID", payResult.ORDERID);
                        dicCheckPars.Add("AMOUNT", payResult.AMOUNT);
                        dicCheckPars.Add("WAITTIME", payResult.WAITTIME);
                        dicCheckPars.Add("TRACEID", payResult.TRACEID);
                        string strCheckPars = StringHelper.GetPars(dicCheckPars);
                        if (NotifyCheck(strCheckPars, payResult.SIGN, StaticPayInfo.pubKey))
                        {
                            messageModel.Success = true;
                            messageModel.Msg = "支付成功";
                        }
                        else
                        {
                            messageModel.Success = false;
                            messageModel.Msg = "签名失败";
                        }
                        break;
                    case "N":
                        messageModel.Success = false;
                        messageModel.Msg = "支付失败";
                        break;
                    case "U":
                    case "Q":
                        int waittime = payResult.WAITTIME.ObjToInt();
                        if (waittime <= 0) waittime = 5;//如果需要等待默认等待5秒后再次查询
                        Thread.Sleep(waittime * 1000);
                        //轮询查询
                        messageModel = await PayCheck(payModel, 1);
                        break;
                    default:
                        messageModel.Success = false;
                        messageModel.Msg = "支付失败";
                        break;
                }
                messageModel.Response.ORDERID = payResult.ORDERID;
                messageModel.Response.ERRCODE = payResult.ERRCODE;
                messageModel.Response.ERRMSG = payResult.ERRMSG;
                messageModel.Response.TRACEID = payResult.TRACEID;
                messageModel.Response.AMOUNT = payResult.AMOUNT;
                messageModel.Response.QRCODETYPE = payResult.QRCODETYPE;
            }
            catch (Exception ex)
            {
                messageModel.Success = false;
                messageModel.Msg = "服务错误";
                messageModel.Response.ERRMSG = ex.Message;
                _logger.LogInformation($"异常信息:{ex.Message}");
                _logger.LogInformation($"异常堆栈:{ex.StackTrace}");
            }
            finally
            {
                _logger.LogInformation($"返回数据->{JsonHelper.GetJson<MessageModel<PayReturnResultModel>>(messageModel)}");
                _logger.LogInformation("支付结束");
            }

            return messageModel;
        }

        public async Task<MessageModel<PayRefundReturnResultModel>> PayRefund(PayRefundNeedModel payModel)
        {
            _logger.LogInformation("退款开始");
            MessageModel<PayRefundReturnResultModel> messageModel = new MessageModel<PayRefundReturnResultModel>();
            messageModel.Response = new PayRefundReturnResultModel();
            try
            {
                _logger.LogInformation($"原始GET参数->{_httpContextAccessor.HttpContext.Request.QueryString}");

                string REQUEST_SN = StringHelper.GetGuidToLongID().ToString().Substring(0, 16);//请求序列码
                string CUST_ID = StaticPayInfo.MERCHANTID;//商户号
                string USER_ID = StaticPayInfo.USER_ID;//操作员号
                string PASSWORD = StaticPayInfo.PASSWORD;//密码
                string TX_CODE = "5W1004";//交易码
                string LANGUAGE = "CN";//语言
                                       //string SIGN_INFO = "";//签名信息
                                       //string SIGNCERT = "";//签名CA信息
                                       //外联平台客户端服务部署的地址+设置的监听端口
                string sUrl = StaticPayInfo.OutAddress;

                //XML请求报文
                //string sRequestMsg = $" requestXml=<?xml version=\"1.0\" encoding=\"GB2312\" standalone=\"yes\" ?><TX><REQUEST_SN>{REQUEST_SN}</REQUEST_SN><CUST_ID>{CUST_ID}</CUST_ID><USER_ID>{USER_ID}</USER_ID><PASSWORD>{PASSWORD}</PASSWORD><TX_CODE>{TX_CODE}</TX_CODE><LANGUAGE>{LANGUAGE}</LANGUAGE><TX_INFO><MONEY>{payModel.MONEY}</MONEY><ORDER>{payModel.ORDER}</ORDER><REFUND_CODE>{payModel.REFUND_CODE}</REFUND_CODE></TX_INFO><SIGN_INFO></SIGN_INFO><SIGNCERT></SIGNCERT></TX> ";
                string sRequestMsg = $"<?xml version=\"1.0\" encoding=\"GB2312\" standalone=\"yes\" ?><TX><REQUEST_SN>{REQUEST_SN}</REQUEST_SN><CUST_ID>{CUST_ID}</CUST_ID><USER_ID>{USER_ID}</USER_ID><PASSWORD>{PASSWORD}</PASSWORD><TX_CODE>{TX_CODE}</TX_CODE><LANGUAGE>{LANGUAGE}</LANGUAGE><TX_INFO><MONEY>{payModel.MONEY}</MONEY><ORDER>{payModel.ORDER}</ORDER><REFUND_CODE>{payModel.REFUND_CODE}</REFUND_CODE></TX_INFO><SIGN_INFO></SIGN_INFO><SIGNCERT></SIGNCERT></TX> ";

                //string sRequestMsg = readRequestFile("E:/02-外联平台/06-测试/测试报文/商户网银/客户端连接-5W1001-W06.txt");


                //注意：请求报文必须放在requestXml参数送
                sRequestMsg = "requestXml=" + sRequestMsg;

                _logger.LogInformation("请求地址：" + sUrl);
                _logger.LogInformation("请求报文：" + sRequestMsg);

                HttpClient request = new HttpClient();
                byte[] byteRquest = Encoding.GetEncoding("GB18030").GetBytes(sRequestMsg);
                ByteArrayContent bytemsg = new ByteArrayContent(byteRquest);
                HttpResponseMessage resulthd = await request.PostAsync(sUrl, bytemsg);
                Stream result = await resulthd.Content.ReadAsStreamAsync();

                StreamReader readerResult = new StreamReader(result, Encoding.GetEncoding("GB18030"));
                string sResult = await readerResult.ReadToEndAsync();
                _logger.LogInformation("响应报文:" + sResult);
                var Xmlresult = XmlHelper.ParseFormByXml<PayRefundReturnModel>(sResult, "TX");
                if (Xmlresult.RETURN_CODE.Equals("000000"))
                {
                    messageModel.Success = true;
                    messageModel.Msg = "退款成功";
                }
                else
                {
                    messageModel.Success = false;
                    messageModel.Msg = "退款失败";
                }
                messageModel.Response.RETURN_MSG = Xmlresult.RETURN_MSG;
                messageModel.Response.TX_CODE = Xmlresult.TX_CODE;
                messageModel.Response.REQUEST_SN = Xmlresult.REQUEST_SN;
                messageModel.Response.RETURN_CODE = Xmlresult.RETURN_CODE;
                messageModel.Response.CUST_ID = Xmlresult.CUST_ID;
                messageModel.Response.LANGUAGE = Xmlresult.LANGUAGE;

                messageModel.Response.AMOUNT = Xmlresult.TX_INFO?.AMOUNT;
                messageModel.Response.PAY_AMOUNT = Xmlresult.TX_INFO?.PAY_AMOUNT;
                messageModel.Response.ORDER_NUM = Xmlresult.TX_INFO?.ORDER_NUM;
                request.Dispose();
            }
            catch (Exception ex)
            {
                messageModel.Success = false;
                messageModel.Msg = "服务错误";
                messageModel.Response.RETURN_MSG = ex.Message;
                _logger.LogInformation($"异常信息:{ex.Message}");
                _logger.LogInformation($"异常堆栈:{ex.StackTrace}");
            }
            finally
            {
                _logger.LogInformation($"返回数据->{JsonHelper.GetJson<MessageModel<PayRefundReturnResultModel>>(messageModel)}");
                _logger.LogInformation("退款结束");

            }
            return messageModel;
        }

        public async Task<MessageModel<PayReturnResultModel>> PayCheck(PayNeedModel payModel, int times)
        {
            _logger.LogInformation("轮询开始");

            MessageModel<PayReturnResultModel> messageModel = new MessageModel<PayReturnResultModel>();
            messageModel.Response = new PayReturnResultModel();
            string url = string.Empty;
            string param = string.Empty;
            string returnData = string.Empty;

            try
            {
                //设置最大轮询次数,跟建行保持一致
                int theLastTime = 6;
                if (times > theLastTime) throw new Exception($"轮询次数超过最大次数{theLastTime}");

                string host = "https://ibsbjstar.ccb.com.cn/CCBIS/B2CMainPlat_00_BEPAY?";

                Dictionary<string, object> dic = new Dictionary<string, object>();

                dic.Add("MERCHANTID", StaticPayInfo.MERCHANTID);// => self::MERCHANTID, // 商户号
                dic.Add("POSID", StaticPayInfo.POSID);// => self::POSID, // 柜台号
                dic.Add("BRANCHID", StaticPayInfo.BRANCHID);// => self::BRANCHID, // 分行号
                dic.Add("TXCODE", "PAY101");// => 'PAY100', // 交易码
                dic.Add("QRYTIME", times.ToString());// => '', // 查询此时(每次加1)
                dic.Add("MERFLAG", "1");// => '', // 商户类型
                dic.Add("ORDERID", payModel.ORDERID);// => '', // 订单号
                dic.Add("QRCODE", payModel.QRCODE);// => '', // 码信息（一维码、二维码）


                //dic.Add("GROUPMCH", "");// => '', // 集团商户信息 
                //dic.Add("QRCODETYPE", "");// => '', // 支付类型1：龙支付 2：微信 3：支付宝 4：银联  
                //dic.Add("TERMNO1", "");// => '', // 终端编号 1
                //dic.Add("TERMNO2", "");// => '', // 终端编号 2 
                //dic.Add("AMOUNT", "");// => '0.01', // 订单金额，单位：元
                //dic.Add("PROINFO", "");// => '', // 商品名称
                //dic.Add("REMARK1", "");// => '', // 备注 1
                //dic.Add("REMARK2", "");// => '', // 备注 2
                //dic.Add("FZINFO1", "");// => '', // 分账信息一
                //dic.Add("FZINFO2", "");// => '', // 分账信息二
                //dic.Add("SUB_APPID", "");// => '', // 子商户公众账号 ID
                //dic.Add("RETURN_FIELD", "");// => '', // 返回信息位图
                //dic.Add("USERPARAM", "");// => '', // 实名支付
                //dic.Add("detail", "");// => '', // 商品详情
                //dic.Add("goods_tag", "");// => '', // 订单优惠标记

                //商户信息
                Dictionary<string, object> dicInfo = new Dictionary<string, object>();
                dicInfo.Add("MERCHANTID", StaticPayInfo.MERCHANTID);// => self::MERCHANTID, // 商户号
                dicInfo.Add("POSID", StaticPayInfo.POSID);// => self::POSID, // 柜台号
                dicInfo.Add("BRANCHID", StaticPayInfo.BRANCHID);// => self::BRANCHID, // 分行号
                var Info = StringHelper.GetPars(dicInfo);

                //var newDic = dic.OrderBy(t => t.Key).ToDictionary(o => o.Key, p => p.Value);
                //参数信息
                param = StringHelper.GetPars(dic);
                //加密
                var paramEncryption = new CCBPayUtil().makeCCBParam(param, StaticPayInfo.pubKey);
                //拼接请求串
                url = host + Info + "&ccbParam=" + paramEncryption;
                //请求
                _logger.LogInformation($"请求地址->{url}");
                _logger.LogInformation($"请求参数->{param}");
                //转换数据
                PayResultModel payResult;
                try
                {
                    returnData = await HttpHelper.PostAsync(url);
                    _logger.LogInformation($"响应数据->{returnData}");
                }
                catch (Exception ex)
                {
                    _logger.LogInformation($"异常信息:{ex.Message}");
                    _logger.LogInformation($"异常堆栈:{ex.StackTrace}");
                    return await PayCheck(payModel, ++times);
                }


                try
                {
                    payResult = JsonHelper.ParseFormByJson<PayResultModel>(returnData);
                }
                catch
                {
                    payResult = new PayResultModel { RESULT = "N", ERRMSG = "参数错误", ORDERID = payModel.ORDERID, AMOUNT = payModel.AMOUNT };
                }

                switch (payResult.RESULT)
                {
                    case "Y":
                        Dictionary<string, object> dicCheckPars = new Dictionary<string, object>();
                        dicCheckPars.Add("RESULT", payResult.RESULT);
                        dicCheckPars.Add("ORDERID", payResult.ORDERID);
                        dicCheckPars.Add("AMOUNT", payResult.AMOUNT);
                        dicCheckPars.Add("WAITTIME", payResult.WAITTIME);
                        string strCheckPars = StringHelper.GetPars(dicCheckPars);
                        if (NotifyCheck(strCheckPars, payResult.SIGN, StaticPayInfo.pubKey))
                        {
                            messageModel.Success = true;
                            messageModel.Msg = "支付成功";
                        }
                        else
                        {
                            messageModel.Success = false;
                            messageModel.Msg = "签名失败";
                        }
                        break;
                    case "N":
                        messageModel.Success = false;
                        messageModel.Msg = "支付失败";
                        break;
                    case "U":
                    case "Q":
                        int waittime = payResult.WAITTIME.ObjToInt();
                        if (waittime <= 0) waittime = 5;//如果需要等待默认等待5秒后再次查询
                        Thread.Sleep(waittime * 1000);
                        //改成轮询查询
                        messageModel = await PayCheck(payModel, ++times);
                        break;
                    default:
                        messageModel.Success = false;
                        messageModel.Msg = "支付失败";
                        break;
                }
                messageModel.Response.ORDERID = payResult.ORDERID;
                messageModel.Response.ERRCODE = payResult.ERRCODE;
                messageModel.Response.ERRMSG = payResult.ERRMSG;
                messageModel.Response.TRACEID = payResult.TRACEID;
                messageModel.Response.AMOUNT = payResult.AMOUNT;
                messageModel.Response.QRCODETYPE = payResult.QRCODETYPE;
            }
            catch (Exception ex)
            {
                messageModel.Success = false;
                messageModel.Msg = "服务错误";
                messageModel.Response.ERRMSG = ex.Message;
                _logger.LogInformation($"异常信息:{ex.Message}");
                _logger.LogInformation($"异常堆栈:{ex.StackTrace}");
            }
            finally
            {
                _logger.LogInformation($"返回数据->{JsonHelper.GetJson<MessageModel<PayReturnResultModel>>(messageModel)}");
                _logger.LogInformation("轮序结束");
            }
            return messageModel;
        }

        public bool NotifyCheck(string strSrc, string sign, string pubKey)
        {
            return new CCBPayUtil().verifyNotifySign(strSrc, sign, pubKey);
        }
    }
}
