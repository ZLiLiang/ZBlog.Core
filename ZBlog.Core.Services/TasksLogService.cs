using System.Linq.Expressions;
using SqlSugar;
using ZBlog.Core.Common.Extensions;
using ZBlog.Core.IServices;
using ZBlog.Core.Model;
using ZBlog.Core.Model.Models;
using ZBlog.Core.Services.Base;

namespace ZBlog.Core.Services
{
    public partial class TasksLogService : BaseService<TasksLog>, ITasksLogService
    {
        public async Task<PageModel<TasksLog>> GetTaskLogs(long jobId, int page, int intPageSize, DateTime? runTime, DateTime? endTime)
        {
            RefAsync<int> totalCount = 0;
            Expression<Func<TasksLog, bool>> whereExpression = log => true;
            if (jobId > 0)
                whereExpression = whereExpression.And(log => log.JobId == jobId);

            var data = await this.Db.Queryable<TasksLog>()
                .LeftJoin<TasksQz>((log, qz) => log.JobId == qz.Id)
                .OrderByDescending(log => log.RunTime)
                .WhereIF(jobId > 0, log => jobId == jobId)
                .WhereIF(runTime != null, log => log.RunTime >= runTime.Value)
                .WhereIF(endTime != null, log => log.RunTime <= endTime.Value)
                .Select((log, qz) => new TasksLog
                {
                    RunPars = log.RunPars,
                    RunResult = log.RunResult,
                    RunTime = log.RunTime,
                    EndTime = log.EndTime,
                    ErrMessage = log.ErrMessage,
                    ErrStackTrace = log.ErrStackTrace,
                    TotalTime = log.TotalTime,
                    Name = qz.Name,
                    JobGroup = qz.JobGroup
                })
                .ToPageListAsync(page, intPageSize, totalCount);

            return new PageModel<TasksLog>(page, totalCount, intPageSize, data);
        }

        /// <summary>
        /// 获取报表信息
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="runTime"></param>
        /// <param name="endTime"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<object> GetTaskOverview(long jobId, DateTime? runTime, DateTime? endTime, string type)
        {
            //按年
            if ("year".Equals(type))
            {

                var year = endTime.Value.Year - runTime.Value.Year;
                var yearList = new List<DateTime>();
                while (year >= 0)
                {
                    yearList.Add(new DateTime(runTime.Value.Year + year, 1, 1));
                    year--;
                }
                var queryableLeft = this.Db.Reportable(yearList)
                    .ToQueryable<DateTime>();
                var queryableRight = this.Db.Queryable<TasksLog>()
                    .Where((x) => x.RunTime.Year >= runTime.Value.Year && x.RunTime.Year <= endTime.Value.Year); //声名表

                var list = this.Db.Queryable(queryableLeft, queryableRight, JoinType.Left,
                    (x1, x2) => x1.ColumnName.Year == x2.RunTime.Year)
                    .GroupBy((x1, x2) => x1.ColumnName)
                    .Select((x1, x2) => new
                    {
                        执行次数 = SqlFunc.AggregateSum(SqlFunc.IIF(x2.Id > 0, 1, 0)),
                        date = x1.ColumnName.Year.ToString() + "年"
                    })
                    .ToList()
                    .OrderBy(t => t.date);

                return list;
            }
            else if ("month".Equals(type))
            {
                //按月
                var queryableLeft = this.Db.Reportable(ReportableDateType.MonthsInLast1years)
                    .ToQueryable<DateTime>(); //生成月份 //ReportableDateType.MonthsInLast1yea 表式近一年月份 并且queryable之后还能在where过滤
                var queryableRight = this.Db.Queryable<TasksLog>()
                    .Where((x) => x.RunTime.Year == runTime.Value.Year); //声名表

                //月份和表JOIN
                var list = queryableLeft
                   .LeftJoin(queryableRight, (x1, x2) => x2.RunTime.ToString("MM月") == x1.ColumnName.ToString("MM月"))
                   .GroupBy((x1, x2) => x1.ColumnName)
                   .Select((x1, x2) => new
                   {
                       //null的数据要为0所以不能用count
                       执行次数 = SqlFunc.AggregateSum(SqlFunc.IIF(x2.Id > 0, 1, 0)),
                       date = x1.ColumnName.ToString("MM月")
                   })
                   .ToList()
                   .OrderBy(t => t.date);
                await Task.CompletedTask;

                return list;
            }
            else if ("day".Equals(type))
            {
                //按日
                var time = runTime.Value;
                var days = DateTime.DaysInMonth(time.Year, time.Month);
                var dayArray = Enumerable.Range(1, days)
                    .Select(it => Convert.ToDateTime(time.ToString("yyyy-MM-" + it)))
                    .ToList();//转成时间数组

                var queryableLeft = this.Db.Reportable(dayArray)
                    .ToQueryable<DateTime>();
                var star = Convert.ToDateTime(runTime.Value.ToString("yyyy-MM-01 00:00:00"));
                var end = Convert.ToDateTime(runTime.Value.ToString($"yyyy-MM-{days} 23:59:59"));
                var queryableRight = this.Db.Queryable<TasksLog>()
                    .Where((x) => x.RunTime >= star && x.RunTime <= end); ; ; //声名表

                var list = this.Db.Queryable(queryableLeft, queryableRight, JoinType.Left,
                    (x1, x2) => x1.ColumnName.Date == x2.RunTime.Date)
                    .GroupBy((x1, x2) => x1.ColumnName)
                    .Select((x1, x2) => new
                    {
                        执行次数 = SqlFunc.AggregateSum(SqlFunc.IIF(x2.Id > 0, 1, 0)),
                        date = x1.ColumnName.Day
                    })
                    .ToList()
                    .OrderBy(t => t.date);
                await Task.CompletedTask;

                return list;
            }
            else if ("hour".Equals(type))
            {
                //按小时
                var time = runTime.Value;
                var days = 24;
                var dayArray = Enumerable.Range(0, days)
                    .Select(it => Convert.ToDateTime(time.ToString($"yyyy-MM-dd {it.ToString().PadLeft(2, '0')}:00:00")))
                    .ToList();//转成时间数组

                var queryableLeft = this.Db.Reportable(dayArray)
                    .ToQueryable<DateTime>();
                var queryableRight = this.Db.Queryable<TasksLog>()
                    .Where((x) => x.RunTime >= runTime.Value.Date && x.RunTime <= runTime.Value.Date.AddDays(1).AddMilliseconds(-1)); //声名表

                var list = this.Db.Queryable(queryableLeft, queryableRight, JoinType.Left,
                    (x1, x2) => x1.ColumnName.Hour == x2.RunTime.Hour)
                    .GroupBy((x1, x2) => x1.ColumnName)
                    .Select((x1, x2) => new
                    {
                        执行次数 = SqlFunc.AggregateSum(SqlFunc.IIF(x2.Id > 0, 1, 0)),
                        date = x1.ColumnName.Hour
                    })
                    .ToList()
                    .OrderBy(t => t.date);
                await Task.CompletedTask;

                return list;
            }
            await Task.CompletedTask;

            return null;
        }
    }
}
