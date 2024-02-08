using Quartz;
using ZBlog.Core.Common.Helper;
using ZBlog.Core.IServices;
using ZBlog.Core.Model.Models;

namespace ZBlog.Core.Tasks
{
    public class JobBase
    {
        public ITasksQzService _tasksQzService;
        public ITasksLogService _tasksLogService;

        public JobBase(ITasksQzService tasksQzService, ITasksLogService tasksLogService)
        {
            _tasksQzService = tasksQzService;
            _tasksLogService = tasksLogService;
        }

        /// <summary>
        /// 执行指定任务
        /// </summary>
        /// <param name="context"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public async Task<string> ExecuteJob(IJobExecutionContext context, Func<Task> func)
        {
            //记录Job
            TasksLog tasksLog = new TasksLog();
            //JobId
            int jobId = context.JobDetail.Key.Name.ObjToInt();
            //Job组名
            string groupName = context.JobDetail.Key.Group;
            //日志
            tasksLog.JobId = jobId;
            tasksLog.RunTime = DateTime.Now;
            string jobHistory = $"【{tasksLog.RunTime.ToString("yyyy-MM-dd HH:mm:ss")}】【执行开始】【Id：{jobId}，组别：{groupName}】";

            try
            {
                await func();//执行任务
                tasksLog.EndTime = DateTime.Now;
                tasksLog.RunResult = true;
                jobHistory += $"，【{tasksLog.EndTime.ToString("yyyy-MM-dd HH:mm:ss")}】【执行成功】";

                JobDataMap jobPars = context.JobDetail.JobDataMap;
                tasksLog.RunPars = jobPars.GetString("JobParam");
            }
            catch (Exception ex)
            {
                tasksLog.EndTime = DateTime.Now;
                tasksLog.RunResult = false;
                tasksLog.ErrMessage = ex.Message;
                tasksLog.ErrMessage = ex.StackTrace;
                jobHistory += $"，【{tasksLog.EndTime.ToString("yyyy-MM-dd HH:mm:ss")}】【执行失败:{ex.Message}】";
            }
            finally
            {
                tasksLog.TotalTime = Math.Round((tasksLog.EndTime - tasksLog.RunTime).TotalSeconds, 3);
                jobHistory += $"(耗时:{tasksLog.TotalTime}秒)";

                if (_tasksQzService != null)
                {
                    var model = await _tasksQzService.QueryById(jobId);
                    if (model != null)
                    {
                        if (_tasksLogService != null)
                            await _tasksLogService.Add(tasksLog);
                        model.RunTimes += 1;
                        if (model.TriggerType == 0)
                            model.CycleHasRunTimes += 1;
                        if (model.TriggerType == 0 && model.CycleRunTimes != 0 && model.CycleHasRunTimes >= model.CycleRunTimes)
                            model.IsStart = false;//循环完善,当循环任务完成后,停止该任务,防止下次启动再次执行
                        var separator = "<br>";
                        // 这里注意数据库字段的长度问题，超过限制，会造成数据库remark不更新问题。
                        model.Remark = $"{jobHistory}{separator}{string.Join(separator, StringHelper.GetTopDataBySeparator(model.Remark, separator, 9))}";
                        await _tasksQzService.Update(model);
                    }
                }
            }

            Console.Out.WriteLine(jobHistory);
            return jobHistory;
        }
    }
}
