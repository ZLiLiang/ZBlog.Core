using Serilog.Context;
using SqlSugar;
using ZBlog.Core.Common.Helper;

namespace ZBlog.Core.Common.LogHelper
{
    public class LogContextExtension : IDisposable
    {
        private readonly Stack<IDisposable> _disposableStack = new Stack<IDisposable>();

        public static LogContextExtension Create => new();

        public void AddStock(IDisposable disposable)
        {
            _disposableStack.Push(disposable);
        }

        public IDisposable SqlAopPushProperty(ISqlSugarClient db)
        {
            AddStock(LogContext.PushProperty(LogContextStatic.LogSource, LogContextStatic.AopSql));
            AddStock(LogContext.PushProperty(LogContextStatic.SqlOutToConsole,
                AppSettings.App(new string[] { "AppSettings", "SqlAOP", "LogToConsole", "Enabled" }).ObjToBool()));
            AddStock(LogContext.PushProperty(LogContextStatic.SqlOutToFile,
                AppSettings.App(new string[] { "AppSettings", "SqlAOP", "LogToFile", "Enabled" }).ObjToBool()));
            AddStock(LogContext.PushProperty(LogContextStatic.OutToDb,
                AppSettings.App(new string[] { "AppSettings", "SqlAOP", "LogToDB", "Enabled" }).ObjToBool()));

            AddStock(LogContext.PushProperty(LogContextStatic.SugarActionType, db.SugarActionType));

            return this;
        }

        public void Dispose()
        {
            while (_disposableStack.Count > 0)
            {
                _disposableStack.Pop().Dispose();
            }
        }
    }
}
