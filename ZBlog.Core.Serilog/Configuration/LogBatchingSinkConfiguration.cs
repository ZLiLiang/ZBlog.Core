using Serilog;
using Serilog.Sinks.PeriodicBatching;
using ZBlog.Core.Common.Helper;
using ZBlog.Core.Serilog.Sink;

namespace ZBlog.Core.Serilog.Configuration
{
    public static class LogBatchingSinkConfiguration
    {
        public static LoggerConfiguration WriteToLogBatching(this LoggerConfiguration loggerConfiguration)
        {
            if (!AppSettings.App("AppSettings", "LogToDb").ObjToBool())
                return loggerConfiguration;

            var exampleSink = new LogBatchingSink();
            var batchingOptions = new PeriodicBatchingSinkOptions
            {
                BatchSizeLimit = 500,
                Period = TimeSpan.FromSeconds(1),
                EagerlyEmitFirstEvent = true,
                QueueLimit = 10000
            };
            var batchingSink = new PeriodicBatchingSink(exampleSink, batchingOptions);

            return loggerConfiguration.WriteTo.Sink(batchingSink);
        }
    }
}
