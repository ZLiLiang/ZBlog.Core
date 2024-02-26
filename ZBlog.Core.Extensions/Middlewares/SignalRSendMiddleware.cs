using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using ZBlog.Core.Common.Helper;
using ZBlog.Core.Common.Hubs;
using ZBlog.Core.Common.LogHelper;

namespace ZBlog.Core.Extensions.Middlewares
{
    public class SignalRSendMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHubContext<ChatHub> _hubContext;

        public SignalRSendMiddleware(RequestDelegate next, IHubContext<ChatHub> hubContext)
        {
            _next = next;
            _hubContext = hubContext;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (AppSettings.App("Middleware", "SignalR", "Enabled").ObjToBool())
            {
                //TODO 主动发送错误消息
                await _hubContext.Clients.All.SendAsync("ReceiveUpdate", LogLock.GetLogData());
            }

            await _next(context);
        }
    }
}
