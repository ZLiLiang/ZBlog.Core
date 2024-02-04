using ZBlog.Core.IServices.Base;
using ZBlog.Core.Model.Models;
using ZBlog.Core.Model;

namespace ZBlog.Core.IServices
{
    /// <summary>
	/// ITasksLogService
	/// </summary>	
    public interface ITasksLogService : IBaseService<TasksLog>
    {
        Task<PageModel<TasksLog>> GetTaskLogs(long jobId, int page, int intPageSize, DateTime? runTime, DateTime? endTime);
        Task<object> GetTaskOverview(long jobId, DateTime? runTime, DateTime? endTime, string type);
    }
}
