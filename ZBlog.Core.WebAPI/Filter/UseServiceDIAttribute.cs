using Microsoft.AspNetCore.Mvc.Filters;
using ZBlog.Core.IServices;

namespace ZBlog.Core.WebAPI.Filter
{
    public class UseServiceDIAttribute : ActionFilterAttribute
    {
        protected readonly ILogger<UseServiceDIAttribute> _logger;
        private readonly IBlogArticleService _blogArticleService;
        private readonly string _name;

        public UseServiceDIAttribute(ILogger<UseServiceDIAttribute> logger, IBlogArticleService blogArticleService, string name)
        {
            _logger = logger;
            _blogArticleService = blogArticleService;
            _name = name;
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            var dd = _blogArticleService.Query().Result;
            _logger.LogInformation("测试自定义服务特性");
            Console.WriteLine(_name);
            base.OnActionExecuted(context);
            DeleteSubscriptionFiles();
        }

        private void DeleteSubscriptionFiles()
        {

        }
    }
}
