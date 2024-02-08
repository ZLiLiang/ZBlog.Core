using Quartz;
using ZBlog.Core.IServices;

/// <summary>
/// 这里要注意下，命名空间和程序集是一样的，不然反射不到
/// </summary>
namespace ZBlog.Core.Tasks
{
    public class JobBlogsQuartz : JobBase, IJob
    {
        private readonly IBlogArticleService _blogArticleService;

        public JobBlogsQuartz(IBlogArticleService blogArticleService, ITasksQzService tasksQzService, ITasksLogService tasksLogService) :
            base(tasksQzService, tasksLogService)
        {
            _blogArticleService = blogArticleService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var executeLog = await ExecuteJob(context, async () => await Run(context));
        }

        public async Task Run(IJobExecutionContext context)
        {
            System.Console.WriteLine($"JobBlogsQuartz 执行 {DateTime.Now.ToShortTimeString()}");
            var list = await _blogArticleService.Query();
            // 也可以通过数据库配置，获取传递过来的参数
            JobDataMap data = context.JobDetail.JobDataMap;
        }
    }
}
