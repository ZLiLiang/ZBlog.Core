using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZBlog.Core.Common.GlobalVars;
using ZBlog.Core.IServices;
using ZBlog.Core.Model.Models;
using ZBlog.Core.Model;
using ZBlog.Core.Repository.UnitOfWorks;
using ZBlog.Core.Tasks.QuartzNet;
using System.Linq.Expressions;
using ZBlog.Core.Common.Helper;
using ZBlog.Core.Services;
using ZBlog.Core.Model.ViewModels;
using Quartz;
using System.Reflection;

namespace ZBlog.Core.WebAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Permissions.Name)]
    public class TasksQzController : ControllerBase
    {
        private readonly ITasksQzService _tasksQzService;
        private readonly ITasksLogService _tasksLogService;
        private readonly ISchedulerCenter _schedulerCenter;
        private readonly IUnitOfWorkManage _unitOfWorkManage;

        public TasksQzController(ITasksQzService tasksQzService, ITasksLogService tasksLogService, ISchedulerCenter schedulerCenter, IUnitOfWorkManage unitOfWorkManage)
        {
            _tasksQzService = tasksQzService;
            _tasksLogService = tasksLogService;
            _schedulerCenter = schedulerCenter;
            _unitOfWorkManage = unitOfWorkManage;
        }

        /// <summary>
        /// 分页获取
        /// </summary>
        /// <param name="page"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<PageModel<TasksQz>>> Get(int page = 1, string key = "")
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrWhiteSpace(key))
            {
                key = "";
            }
            int intPageSize = 50;

            Expression<Func<TasksQz, bool>> whereExpression = a => a.IsDeleted != true && (a.Name != null && a.Name.Contains(key));

            var data = await _tasksQzService.QueryPage(whereExpression, page, intPageSize, " Id desc ");
            if (data.DataCount > 0)
            {
                foreach (var item in data.Data)
                {
                    item.Triggers = await _schedulerCenter.GetTaskStaus(item);
                }
            }

            return MessageModel<PageModel<TasksQz>>.Message(data.DataCount >= 0, "获取成功", data);
        }

        /// <summary>
        /// 添加计划任务
        /// </summary>
        /// <param name="tasksQz"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MessageModel<string>> Post([FromBody] TasksQz tasksQz)
        {
            var data = new MessageModel<string>();
            _unitOfWorkManage.BeginTran();
            var id = await _tasksQzService.Add(tasksQz);
            data.Success = id > 0;

            try
            {
                if (data.Success)
                {
                    tasksQz.Id = id;
                    data.Response = id.ObjToString();
                    data.Msg = "添加成功";
                    if (tasksQz.IsStart)
                    {
                        //如果是启动自动
                        var resuleModel = await _schedulerCenter.AddScheduleJobAsync(tasksQz);
                        data.Success = resuleModel.Success;
                        if (resuleModel.Success)
                            data.Msg = $"{data.Msg}=>启动成功=>{resuleModel.Msg}";
                        else
                            data.Msg = $"{data.Msg}=>启动失败=>{resuleModel.Msg}";
                    }
                }
                else
                {
                    data.Msg = "添加失败";
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (data.Success)
                    _unitOfWorkManage.CommitTran();
                else
                    _unitOfWorkManage.RollbackTran();
            }

            return data;
        }

        /// <summary>
        /// 修改计划任务
        /// </summary>
        /// <param name="tasksQz"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<MessageModel<string>> Put([FromBody] TasksQz tasksQz)
        {
            var data = new MessageModel<string>();
            if (tasksQz != null && tasksQz.Id > 0)
            {
                _unitOfWorkManage.BeginTran();
                data.Success = await _tasksQzService.Update(tasksQz);
                try
                {
                    if (data.Success)
                    {
                        data.Msg = "修改成功";
                        data.Response = tasksQz?.Id.ObjToString();
                        if (tasksQz.IsStart)
                        {
                            var resuleModelStop = await _schedulerCenter.StopScheduleJobAsync(tasksQz);
                            data.Msg = $"{data.Msg}=>停止:{resuleModelStop.Msg}";
                            var resuleModelStar = await _schedulerCenter.AddScheduleJobAsync(tasksQz);
                            data.Success = resuleModelStar.Success;
                            data.Msg = $"{data.Msg}=>启动:{resuleModelStar.Msg}";
                        }
                        else
                        {
                            var resuleModelStop = await _schedulerCenter.StopScheduleJobAsync(tasksQz);
                            data.Msg = $"{data.Msg}=>停止:{resuleModelStop.Msg}";
                        }
                    }
                    else
                    {
                        data.Msg = "修改失败";
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    if (data.Success)
                        _unitOfWorkManage.CommitTran();
                    else
                        _unitOfWorkManage.RollbackTran();
                }
            }

            return data;
        }

        /// <summary>
        /// 删除一个任务
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<MessageModel<string>> Delete(long jobId)
        {
            var data = new MessageModel<string>();
            var model = await _tasksQzService.QueryById(jobId);

            if (model != null)
            {
                _unitOfWorkManage.BeginTran();
                data.Success = await _tasksQzService.Delete(model);
                try
                {
                    data.Response = jobId.ObjToString();
                    if (data.Success)
                    {
                        data.Msg = "删除成功";
                        var resuleModel = await _schedulerCenter.StopScheduleJobAsync(model);
                        data.Msg = $"{data.Msg}=>任务状态=>{resuleModel.Msg}";
                    }
                    else
                    {
                        data.Msg = "删除失败";
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    if (data.Success)
                        _unitOfWorkManage.CommitTran();
                    else
                        _unitOfWorkManage.RollbackTran();
                }
            }
            else
            {
                data.Msg = "任务不存在";
            }

            return data;
        }

        /// <summary>
        /// 启动计划任务
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<string>> StartJob(long jobId)
        {
            var data = new MessageModel<string>();
            var model = await _tasksQzService.QueryById(jobId);

            if (model != null)
            {
                _unitOfWorkManage.BeginTran();
                try
                {
                    model.IsStart = true;
                    data.Success = await _tasksQzService.Update(model);
                    data.Response = jobId.ObjToString();
                    if (data.Success)
                    {
                        data.Msg = "更新成功";
                        var resuleModel = await _schedulerCenter.AddScheduleJobAsync(model);
                        data.Success = resuleModel.Success;
                        if (resuleModel.Success)
                            data.Msg = $"{data.Msg}=>启动成功=>{resuleModel.Msg}";
                        else
                            data.Msg = $"{data.Msg}=>启动失败=>{resuleModel.Msg}";
                    }
                    else
                    {
                        data.Msg = "更新失败";
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    if (data.Success)
                        _unitOfWorkManage.CommitTran();
                    else
                        _unitOfWorkManage.RollbackTran();
                }
            }
            else
            {
                data.Msg = "任务不存在";
            }

            return data;
        }

        /// <summary>
        /// 停止一个计划任务
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<string>> StopJob(long jobId)
        {
            var data = new MessageModel<string>();
            var model = await _tasksQzService.QueryById(jobId);

            if (model != null)
            {
                model.IsStart = false;
                data.Success = await _tasksQzService.Update(model);
                data.Response = jobId.ObjToString();
                if (data.Success)
                {
                    data.Msg = "更新成功";
                    var resuleModel = await _schedulerCenter.StopScheduleJobAsync(model);
                    if (resuleModel.Success)
                        data.Msg = $"{data.Msg}=>停止成功=>{resuleModel.Msg}";
                    else
                        data.Msg = $"{data.Msg}=>停止失败=>{resuleModel.Msg}";
                }
                else
                {
                    data.Msg = "更新失败";
                }
            }
            else
            {
                data.Msg = "任务不存在";
            }

            return data;
        }

        /// <summary>
        /// 暂停一个计划任务
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<string>> PauseJob(long jobId)
        {
            var data = new MessageModel<string>();
            var model = await _tasksQzService.QueryById(jobId);

            if (model != null)
            {
                _unitOfWorkManage.BeginTran();
                try
                {
                    data.Success = await _tasksQzService.Update(model);
                    data.Response = jobId.ObjToString();
                    if (data.Success)
                    {
                        data.Msg = "更新成功";
                        var resuleModel = await _schedulerCenter.PauseJob(model);
                        if (resuleModel.Success)
                        {
                            data.Msg = $"{data.Msg}=>暂停成功=>{resuleModel.Msg}";
                        }
                        else
                        {
                            data.Msg = $"{data.Msg}=>暂停失败=>{resuleModel.Msg}";
                        }
                        data.Success = resuleModel.Success;
                    }
                    else
                    {
                        data.Msg = "更新失败";
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    if (data.Success)
                        _unitOfWorkManage.CommitTran();
                    else
                        _unitOfWorkManage.RollbackTran();
                }
            }
            else
            {
                data.Msg = "任务不存在";
            }

            return data;
        }

        /// <summary>
        /// 恢复一个计划任务
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<string>> ResumeJob(long jobId)
        {
            var data = new MessageModel<string>();
            var model = await _tasksQzService.QueryById(jobId);

            if (model != null)
            {
                _unitOfWorkManage.BeginTran();
                try
                {
                    model.IsStart = true;
                    data.Success = await _tasksQzService.Update(model);
                    data.Response = jobId.ObjToString();
                    if (data.Success)
                    {
                        data.Msg = "更新成功";
                        var resuleModel = await _schedulerCenter.ResumeJob(model);
                        if (resuleModel.Success)
                        {
                            data.Msg = $"{data.Msg}=>恢复成功=>{resuleModel.Msg}";
                        }
                        else
                        {
                            data.Msg = $"{data.Msg}=>恢复失败=>{resuleModel.Msg}";
                        }
                        data.Success = resuleModel.Success;
                    }
                    else
                    {
                        data.Msg = "更新失败";
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    if (data.Success)
                        _unitOfWorkManage.CommitTran();
                    else
                        _unitOfWorkManage.RollbackTran();
                }
            }
            else
            {
                data.Msg = "任务不存在";
            }

            return data;
        }

        /// <summary>
        /// 重启一个计划任务
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<string>> ReCovery(long jobId)
        {
            var data = new MessageModel<string>();
            var model = await _tasksQzService.QueryById(jobId);
            if (model != null)
            {

                _unitOfWorkManage.BeginTran();
                try
                {
                    model.IsStart = true;
                    data.Success = await _tasksQzService.Update(model);
                    data.Response = jobId.ObjToString();
                    if (data.Success)
                    {
                        data.Msg = "更新成功";
                        var resuleModelStop = await _schedulerCenter.StopScheduleJobAsync(model);
                        var resuleModelStar = await _schedulerCenter.AddScheduleJobAsync(model);
                        if (resuleModelStar.Success)
                        {
                            data.Msg = $"{data.Msg}=>停止:{resuleModelStop.Msg}=>启动:{resuleModelStar.Msg}";
                            data.Response = jobId.ObjToString();

                        }
                        else
                        {
                            data.Msg = $"{data.Msg}=>停止:{resuleModelStop.Msg}=>启动:{resuleModelStar.Msg}";
                            data.Response = jobId.ObjToString();
                        }
                        data.Success = resuleModelStar.Success;
                    }
                    else
                    {
                        data.Msg = "更新失败";
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    if (data.Success)
                        _unitOfWorkManage.CommitTran();
                    else
                        _unitOfWorkManage.RollbackTran();
                }
            }
            else
            {
                data.Msg = "任务不存在";
            }

            return data;
        }

        /// <summary>
        /// 获取任务命名空间
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public MessageModel<List<QuartzReflectionViewModel>> GetTaskNameSpace()
        {
            var baseType = typeof(IJob);
            var path = AppDomain.CurrentDomain.RelativeSearchPath ?? AppDomain.CurrentDomain.BaseDirectory;
            var referencedAssemblies = Directory.GetFiles(path, "ZBlog.Core.Tasks.dll")
                .Select(Assembly.LoadFrom)
                .ToArray();
            var types = referencedAssemblies
                .SelectMany(a => a.DefinedTypes)
                .Select(type => type.AsType())
                .Where(x => x != baseType && baseType.IsAssignableFrom(x))
                .ToArray();
            var implementTypes = types.Where(x => x.IsClass)
                .Select(it => new QuartzReflectionViewModel
                {
                    NameSpace = it.Namespace,
                    NameClass = it.Name,
                    Remark = ""
                })
                .ToList();

            return MessageModel<List<QuartzReflectionViewModel>>.SUCCESS("获取成功", implementTypes);
        }

        /// <summary>
        /// 立即执行任务
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<string>> ExecuteJob(long jobId)
        {
            var data = new MessageModel<string>();
            var model = await _tasksQzService.QueryById(jobId);

            if (model != null)
            {
                return await _schedulerCenter.ExecuteJobAsync(model);
            }
            else
            {
                data.Msg = "任务不存在";
            }

            return data;
        }

        /// <summary>
        /// 获取任务运行日志
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="runTimeStart"></param>
        /// <param name="runTimeEnd"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<PageModel<TasksLog>>> GetTaskLogs(
            long jobId,
            int page = 1,
            int pageSize = 10,
            DateTime? runTimeStart = null,
            DateTime? runTimeEnd = null)
        {
            var model = await _tasksLogService.GetTaskLogs(jobId, page, pageSize, runTimeStart, runTimeEnd);

            return MessageModel<PageModel<TasksLog>>.Message(model.DataCount >= 0, "获取成功", model);
        }

        /// <summary>
        /// 任务概况
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="runTimeStart"></param>
        /// <param name="runTimeEnd"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<object>> GetTaskOverview(
            long jobId,
            int page = 1,
            int pageSize = 10,
            DateTime? runTimeStart = null,
            DateTime? runTimeEnd = null,
            string type = "month")
        {
            var model = await _tasksLogService.GetTaskOverview(jobId, runTimeStart, runTimeEnd, type);

            return MessageModel<object>.Message(true, "获取成功", model);
        }
    }
}
