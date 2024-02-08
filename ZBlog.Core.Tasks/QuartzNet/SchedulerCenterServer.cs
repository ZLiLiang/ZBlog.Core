using System.Collections.Specialized;
using System.Reflection;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Triggers;
using Quartz.Spi;
using ZBlog.Core.Common.Helper;
using ZBlog.Core.Model;
using ZBlog.Core.Model.Models;
using ZBlog.Core.Model.ViewModels;

namespace ZBlog.Core.Tasks.QuartzNet
{
    /// <summary>
    /// 任务调度管理中心
    /// </summary>
    public class SchedulerCenterServer : ISchedulerCenter
    {
        private Task<IScheduler> _scheduler;
        private readonly IJobFactory _jobFactory;

        public SchedulerCenterServer(IJobFactory jobFactory)
        {
            _jobFactory = jobFactory;
            _scheduler = GetSchedulerAsync();
        }

        private Task<IScheduler> GetSchedulerAsync()
        {
            if (_scheduler != null)
            {
                return _scheduler;
            }
            else
            {
                // 从Factory中获取Scheduler实例
                NameValueCollection collection = new NameValueCollection
                {
                    { "quartz.serializer.type", "binary" },
                };
                StdSchedulerFactory factory = new StdSchedulerFactory(collection);
                _scheduler = factory.GetScheduler();

                return _scheduler;
            }
        }

        /// <summary>
        /// 开启任务调度
        /// </summary>
        /// <returns></returns>
        public async Task<MessageModel<string>> StartScheduleAsync()
        {

            try
            {
                _scheduler.Result.JobFactory = _jobFactory;
                if (!_scheduler.Result.IsStarted)
                {
                    //等待任务运行完成
                    await _scheduler.Result.Start();
                    await Console.Out.WriteLineAsync("任务调度开启！");
                    var result = new MessageModel<string>
                    {
                        Success = true,
                        Msg = $"任务调度开启成功"
                    };

                    return result;
                }
                else
                {
                    var result = new MessageModel<string>
                    {
                        Success = false,
                        Msg = $"任务调度已经开启"
                    };

                    return result;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// 停止任务调度
        /// </summary>
        /// <returns></returns>
        public async Task<MessageModel<string>> StopScheduleAsync()
        {
            try
            {
                if (_scheduler.Result.IsShutdown)
                {
                    //等待任务运行完成
                    await _scheduler.Result.Shutdown();
                    await Console.Out.WriteLineAsync("任务调度停止！");
                    var result = new MessageModel<string>
                    {
                        Success = true,
                        Msg = $"任务调度停止成功"
                    };

                    return result;
                }
                else
                {
                    var result = new MessageModel<string>
                    {
                        Success = false,
                        Msg = $"任务调度已经停止"
                    };

                    return result;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// 添加一个计划任务（映射程序集指定IJob实现类）
        /// </summary>
        /// <param name="tasksQz"></param>
        /// <returns></returns>
        public async Task<MessageModel<string>> AddScheduleJobAsync(TasksQz tasksQz)
        {
            var result = new MessageModel<string>();
            if (tasksQz != null)
            {
                try
                {
                    JobKey jobKey = new JobKey(tasksQz.Id.ToString(), tasksQz.JobGroup);
                    if (await _scheduler.Result.CheckExists(jobKey))
                    {
                        result.Success = false;
                        result.Msg = $"该任务计划已经在执行:【{tasksQz.Name}】,请勿重复启动！";

                        return result;
                    }

                    if (tasksQz.TriggerType == 0 && tasksQz.CycleRunTimes != 0 && tasksQz.CycleHasRunTimes >= tasksQz.CycleRunTimes)
                    {
                        result.Success = false;
                        result.Msg = $"该任务计划已完成:【{tasksQz.Name}】,无需重复启动,如需启动请修改已循环次数再提交";

                        return result;
                    }

                    #region 设置开始时间和结束时间

                    if (tasksQz.BeginTime == null)
                        tasksQz.BeginTime = DateTime.Now;
                    DateTimeOffset startRunTime = DateBuilder.NextGivenSecondDate(tasksQz.BeginTime, 1);//设置开始时间
                    if (tasksQz.EndTime == null)
                        tasksQz.EndTime = DateTime.MaxValue.AddDays(-1);
                    DateTimeOffset endRunTime = DateBuilder.NextGivenSecondDate(tasksQz.EndTime, 1);

                    #endregion

                    #region 通过反射获取程序集类型和类

                    Assembly assembly = Assembly.Load(new AssemblyName(tasksQz.AssemblyName));
                    Type jobType = assembly.GetType($"{tasksQz.AssemblyName}.{tasksQz.ClassName}");

                    #endregion

                    //判断任务调度是否开启
                    if (!_scheduler.Result.IsStarted)
                        await StartScheduleAsync();

                    //传入反射出来的执行程序集
                    IJobDetail job = new JobDetailImpl(tasksQz.Id.ToString(), tasksQz.JobGroup, jobType);
                    job.JobDataMap.Add("JobParam", tasksQz.JobParams);

                    ITrigger trigger;
                    if (tasksQz.Cron != null && CronExpression.IsValidExpression(tasksQz.Cron) && tasksQz.TriggerType > 0)
                    {
                        trigger = CreateCronTrigger(tasksQz);
                        ((CronTriggerImpl)trigger).MisfireInstruction = MisfireInstruction.CronTrigger.DoNothing;
                    }
                    else
                    {
                        trigger = CreateSimpleTrigger(tasksQz);
                    }

                    // 告诉Quartz使用我们的触发器来安排作业
                    await _scheduler.Result.ScheduleJob(job, trigger);
                    result.Success = true;
                    result.Msg = $"【{tasksQz.Name}】成功";

                    return result;
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Msg = $"任务计划异常:【{ex.Message}】";

                    return result;
                }
            }
            else
            {
                result.Success = false;
                result.Msg = $"任务计划不存在:【{tasksQz?.Name}】";
                return result;
            }
        }

        /// <summary>
        /// 暂停一个指定的计划任务
        /// </summary>
        /// <param name="sysSchedule"></param>
        /// <returns></returns>
        public async Task<MessageModel<string>> StopScheduleJobAsync(TasksQz sysSchedule)
        {
            try
            {
                JobKey jobKey = new JobKey(sysSchedule.Id.ToString(), sysSchedule.JobGroup);
                if (!await _scheduler.Result.CheckExists(jobKey))
                {
                    var result = new MessageModel<string>
                    {
                        Success = false,
                        Msg = $"未找到要暂停的任务:【{sysSchedule.Name}】"
                    };

                    return result;
                }
                else
                {
                    await _scheduler.Result.DeleteJob(jobKey);
                    var result = new MessageModel<string>
                    {
                        Success = true,
                        Msg = $"【{sysSchedule.Name}】成功"
                    };

                    return result;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// 任务是否存在?
        /// </summary>
        /// <param name="sysSchedule"></param>
        /// <returns></returns>
        public async Task<bool> IsExistScheduleJobAsync(TasksQz sysSchedule)
        {
            JobKey jobKey = new JobKey(sysSchedule.Id.ToString(), sysSchedule.JobGroup);
            if (await _scheduler.Result.CheckExists(jobKey))
                return true;
            else
                return false;
        }

        /// <summary>
        /// 暂停指定的计划任务
        /// </summary>
        /// <param name="sysSchedule"></param>
        /// <returns></returns>
        public async Task<MessageModel<string>> PauseJob(TasksQz sysSchedule)
        {
            try
            {
                JobKey jobKey = new JobKey(sysSchedule.Id.ToString(), sysSchedule.JobGroup);
                if (!await _scheduler.Result.CheckExists(jobKey))
                {
                    return new MessageModel<string>
                    {
                        Success = false,
                        Msg = $"未找到要暂停的任务:【{sysSchedule.Name}】"
                    };
                }

                await _scheduler.Result.PauseJob(jobKey);
                return new MessageModel<string>
                {
                    Success = true,
                    Msg = $"【{sysSchedule.Name}】成功"
                };
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// 恢复指定的计划任务
        /// </summary>
        /// <param name="sysSchedule"></param>
        /// <returns></returns>
        public async Task<MessageModel<string>> ResumeJob(TasksQz sysSchedule)
        {
            try
            {
                JobKey jobKey = new JobKey(sysSchedule.Id.ToString(), sysSchedule.JobGroup);
                if (!await _scheduler.Result.CheckExists(jobKey))
                {
                    return new MessageModel<string>
                    {
                        Success = false,
                        Msg = $"未找到要恢复的任务:【{sysSchedule.Name}】"
                    };
                }

                await _scheduler.Result.ResumeJob(jobKey);
                return new MessageModel<string>
                {
                    Success = true,
                    Msg = $"【{sysSchedule.Name}】成功"
                };
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<TaskInfoDto>> GetTaskStaus(TasksQz sysSchedule)
        {
            var taskInfos = new List<TaskInfoDto>();
            var noTask = new List<TaskInfoDto>
            {
                new TaskInfoDto
                {
                    JobId= sysSchedule.Id.ObjToString(),
                    JobGroup=sysSchedule.JobGroup,
                    TriggerId="",
                    TriggerGroup="",
                    TriggerStatus="不存在"
                }
            };
            JobKey jobKey = new JobKey(sysSchedule.Id.ToString(), sysSchedule.JobGroup);
            IJobDetail jobDetail = await _scheduler.Result.GetJobDetail(jobKey);
            if (jobDetail == null)
                return noTask;
            var triggers = await _scheduler.Result.GetTriggersOfJob(jobKey);
            if (triggers == null || triggers.Count == 0)
                return noTask;

            foreach (var trigger in triggers)
            {
                var triggerStaus = await _scheduler.Result.GetTriggerState(trigger.Key);
                var state = GetTriggerState(triggerStaus.ObjToString());
                taskInfos.Add(new TaskInfoDto
                {
                    JobId = jobDetail.Key.Name,
                    JobGroup = jobDetail.Key.Group,
                    TriggerId = trigger.Key.Name,
                    TriggerGroup = trigger.Key.Group,
                    TriggerStatus = state
                });
            }

            return taskInfos;
        }

        public string GetTriggerState(string key)
        {
            string state = null;
            if (key != null)
                key = key.ToUpper();

            switch (key)
            {
                case "1":
                    state = "暂停";
                    break;
                case "2":
                    state = "完成";
                    break;
                case "3":
                    state = "出错";
                    break;
                case "4":
                    state = "阻塞";
                    break;
                case "0":
                    state = "正常";
                    break;
                case "-1":
                    state = "不存在";
                    break;
                case "BLOCKED":
                    state = "阻塞";
                    break;
                case "COMPLETE":
                    state = "完成";
                    break;
                case "ERROR":
                    state = "出错";
                    break;
                case "NONE":
                    state = "不存在";
                    break;
                case "NORMAL":
                    state = "正常";
                    break;
                case "PAUSED":
                    state = "暂停";
                    break;
            }
            return state;
        }

        /// <summary>
        /// 立即执行 一个任务
        /// </summary>
        /// <param name="tasksQz"></param>
        /// <returns></returns>
        public async Task<MessageModel<string>> ExecuteJobAsync(TasksQz tasksQz)
        {
            try
            {
                JobKey jobKey = new JobKey(tasksQz.Id.ToString(), tasksQz.JobGroup);

                //判断任务是否存在，存在则触发一次，不存在则先添加一个任务，触发以后再停止任务
                if (!await _scheduler.Result.CheckExists(jobKey))
                {
                    //不存在 则添加一个计划任务
                    await AddScheduleJobAsync(tasksQz);

                    //触发执行一次
                    await _scheduler.Result.TriggerJob(jobKey);

                    //停止任务
                    await StopScheduleJobAsync(tasksQz);

                    return new MessageModel<string>
                    {
                        Success = true,
                        Msg = $"立即执行计划任务:【{tasksQz.Name}】成功"
                    };
                }
                else
                {
                    await _scheduler.Result.TriggerJob(jobKey);

                    return new MessageModel<string>
                    {
                        Success = true,
                        Msg = $"立即执行计划任务:【{tasksQz.Name}】成功"
                    };
                }
            }
            catch (Exception ex)
            {

                return new MessageModel<string>
                {
                    Msg = $"立即执行计划任务失败:【{ex.Message}】"
                };
            }
        }

        #region 创建触发器帮助方法

        /// <summary>
        /// 创建SimpleTrigger触发器（简单触发器）
        /// </summary>
        /// <param name="tasksQz"></param>
        /// <returns></returns>
        private ITrigger CreateSimpleTrigger(TasksQz tasksQz)
        {
            if (tasksQz.CycleRunTimes > 0)
            {
                ITrigger trigger = TriggerBuilder.Create()
                    .WithIdentity(tasksQz.Id.ToString(), tasksQz.JobGroup)
                    .StartAt(tasksQz.BeginTime.Value)
                    .WithSimpleSchedule(it => it.WithIntervalInSeconds(tasksQz.IntervalSecond)
                                              .WithRepeatCount(tasksQz.CycleRunTimes - 1))
                    .EndAt(tasksQz.EndTime.Value)
                    .Build();

                return trigger;
            }
            else
            {
                ITrigger trigger = TriggerBuilder.Create()
                    .WithIdentity(tasksQz.Id.ToString(), tasksQz.JobGroup)
                    .StartAt(tasksQz.BeginTime.Value)
                    .WithSimpleSchedule(it => it.WithIntervalInSeconds(tasksQz.IntervalSecond)
                                              .RepeatForever())
                    .EndAt(tasksQz.EndTime.Value)
                    .Build();

                return trigger;
            }
            // 触发作业立即运行，然后每10秒重复一次，无限循环
        }

        /// <summary>
        /// 创建类型Cron的触发器
        /// </summary>
        /// <param name="tasksQz"></param>
        /// <returns></returns>
        private ITrigger CreateCronTrigger(TasksQz tasksQz)
        {
            // 作业触发器
            return TriggerBuilder.Create()
                .WithIdentity(tasksQz.Id.ToString(), tasksQz.JobGroup)
                .StartAt(tasksQz.BeginTime.Value)//开始时间
                .EndAt(tasksQz.EndTime.Value)//结束数据
                .WithCronSchedule(tasksQz.Cron)//指定cron表达式
                .ForJob(tasksQz.Id.ToString(), tasksQz.JobGroup)//作业名称
                .Build();
        }

        #endregion
    }
}
