using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZBlog.Core.Common.GlobalVars;
using ZBlog.Core.IServices;
using ZBlog.Core.Model.ViewModels;
using ZBlog.Core.Model;
using ZBlog.Core.Services;

namespace ZBlog.Core.WebAPI.Controllers
{
    /// <summary>
    /// 微信公众号管理 
    /// </summary>   
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Permissions.Name)]
    public class WeChatController : ControllerBase
    {
        private readonly IWeChatConfigService _weChatConfigService;
        private readonly ILogger<WeChatController> _logger;

        public WeChatController(IWeChatConfigService weChatConfigService, ILogger<WeChatController> logger)
        {
            _weChatConfigService = weChatConfigService;
            _logger = logger;
        }

        /// <summary>
        /// 更新Token
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns> 
        [HttpGet]
        public async Task<MessageModel<WeChatApiDto>> GetToken(string id)
        {
            return await _weChatConfigService.GetToken(id);

        }

        /// <summary>
        /// 刷新Token
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns> 
        [HttpGet]
        public async Task<MessageModel<WeChatApiDto>> RefreshToken(string id)
        {
            return await _weChatConfigService.RefreshToken(id);

        }

        /// <summary>
        /// 获取模板
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns> 
        [HttpGet]
        public async Task<MessageModel<WeChatApiDto>> GetTemplate(string id)
        {
            return await _weChatConfigService.GetTemplate(id);
        }

        /// <summary>
        /// 获取菜单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns> 
        [HttpGet]
        public async Task<MessageModel<WeChatApiDto>> GetMenu(string id)
        {
            return await _weChatConfigService.GetMenu(id);
        }

        /// <summary>
        /// 更新菜单
        /// </summary>
        /// <param name="menu"></param>
        /// <returns></returns> 
        [HttpPut]
        public async Task<MessageModel<WeChatApiDto>> UpdateMenu(WeChatApiDto menu)
        {
            return await _weChatConfigService.UpdateMenu(menu);
        }

        /// <summary>
        /// 获取订阅用户(所有)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<WeChatApiDto>> GetSubUsers(string id)
        {
            return await _weChatConfigService.GetSubUsers(id);
        }

        /// <summary>
        /// 入口
        /// </summary>
        /// <param name="validDto"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        [HttpGet]
        public async Task<string> Valid([FromQuery] WeChatValidDto validDto)
        {
            using (var reader = new StreamReader(Request.Body))
            {
                var body = await reader.ReadToEndAsync();
                return await _weChatConfigService.Valid(validDto, body);
            }
        }

        /// <summary>
        /// 获取订阅用户
        /// </summary>
        /// <param name="id"></param>
        /// <param name="openid"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<MessageModel<WeChatApiDto>> GetSubUser(string id, string openid)
        {
            return await _weChatConfigService.GetSubUser(id, openid);
        }

        /// <summary>
        /// 获取一个绑定员工公众号二维码
        /// </summary>
        /// <param name="info">消息</param> 
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<MessageModel<WeChatResponseUserInfo>> GetQRBind([FromQuery] WeChatUserInfo info)
        {
            return await _weChatConfigService.GetQRBind(info);
        }

        /// <summary>
        /// 推送卡片消息接口
        /// </summary>
        /// <param name="msg">卡片消息对象</param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<MessageModel<WeChatResponseUserInfo>> PushCardMsg(WeChatCardMsgDataDto msg)
        {
            string pushUserIP = $"{Request.HttpContext.Connection.RemoteIpAddress}:{Request.HttpContext.Connection.RemotePort}";
            return await _weChatConfigService.PushCardMsg(msg, pushUserIP);
        }

        /// <summary>
        /// 推送卡片消息接口
        /// </summary>
        /// <param name="msg">卡片消息对象</param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<MessageModel<WeChatResponseUserInfo>> PushCardMsgGet([FromQuery] WeChatCardMsgDataDto msg)
        {
            string pushUserIP = $"{Request.HttpContext.Connection.RemoteIpAddress}:{Request.HttpContext.Connection.RemotePort}";
            return await _weChatConfigService.PushCardMsg(msg, pushUserIP);
        }

        /// <summary>
        /// 推送文本消息
        /// </summary>
        /// <param name="msg">消息对象</param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<MessageModel<WeChatApiDto>> PushTxtMsg([FromBody] WeChatPushTestDto msg)
        {
            return await _weChatConfigService.PushTxtMsg(msg);
        }

        /// <summary>
        /// 通过绑定用户获取微信用户信息(一般用于初次绑定检测)
        /// </summary>
        /// <param name="info">信息</param> 
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<MessageModel<WeChatResponseUserInfo>> GetBindUserInfo([FromQuery] WeChatUserInfo info)
        {
            return await _weChatConfigService.GetBindUserInfo(info);
        }

        /// <summary>
        /// 用户解绑
        /// </summary>
        /// <param name="info">消息</param> 
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<MessageModel<WeChatResponseUserInfo>> UnBind([FromQuery] WeChatUserInfo info)
        {
            return await _weChatConfigService.UnBind(info);
        }
    }
}
