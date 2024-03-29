﻿using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Json;
using ZBlog.Core.Serilog.Es.HttpInfo;

namespace ZBlog.Core.Serilog.Es.Formatters
{
    public class LogstashJsonFormatter : ITextFormatter
    {
        private static readonly JsonValueFormatter valueFormatter = new JsonValueFormatter();

        public void Format(LogEvent logEvent, TextWriter output)
        {
            FormatContent(logEvent, output);

            output.WriteLine();
        }

        /// <summary>
        /// 格式化 最终输出到elk的核心部分
        /// </summary>
        /// <param name="logEvent"></param>
        /// <param name="output"></param>
        private static void FormatContent(LogEvent logEvent, TextWriter output)
        {
            if (logEvent == null)
                throw new ArgumentNullException(nameof(logEvent));
            if (output == null)
                throw new ArgumentNullException(nameof(output));

            output.Write('{');

            // 读取相关配置
            var logConfigRootDTOInfo = JsonConfigUtils.GetAppSettings<LogConfigRootDto>(AppSettingsFileNameConfig.AppSettingsFileName, "LogFiedOutPutConfigs");
            if (logConfigRootDTOInfo == null)
                return;

            // 写入所有的项目配置项的字段 在appsetting中配置的 输出elk节点的数据字段
            foreach (var item in logConfigRootDTOInfo.ConfigsInfos)
            {
                switch (item.FiedName)
                {
                    default:
                        WritePropertyAndValue(output, item.FiedName, item.FiedValue);
                        output.Write(",");
                        break;
                }
            }

            // 写入http对应的信息数据
            if (HttpContextProvider.GetCurrent()?.Request != null)
            {
                if (!string.IsNullOrEmpty(HttpContextProvider.GetCurrent().Request.Method))
                {
                    WritePropertyAndValue(output, "method", HttpContextProvider.GetCurrent().Request.Method);
                    output.Write(",");
                }

                // 输出请求页面url
                if (!string.IsNullOrEmpty(HttpContextProvider.GetCurrent().Request.Path))
                {
                    WritePropertyAndValue(output, "requestUrl", HttpContextProvider.GetCurrent().Request.Path.ToString());
                    output.Write(",");
                }

                // 输出携带token
                if (HttpContextProvider.GetCurrent().Request.Headers["Authorization"].FirstOrDefault() != null)
                {
                    WritePropertyAndValue(output, "Authorization", HttpContextProvider.GetCurrent().Request.Headers["Authorization"].FirstOrDefault());
                    output.Write(",");
                }

                // 输出请求参数
                if (!string.IsNullOrEmpty(HttpContextProvider.GetCurrent().Request.Method))
                {
                    string contentFromBody = ParamsHelper.GetParams(HttpContextProvider.GetCurrent());
                    WritePropertyAndValue(output, "requestParam", contentFromBody);
                    output.Write(",");
                }

                // 输出请求方法类型
                if (!string.IsNullOrEmpty(HttpContextProvider.GetCurrent().Request.Method))
                {
                    WritePropertyAndValue(output, "method", HttpContextProvider.GetCurrent().Request.Method);
                    output.Write(",");
                }
            }

            // 输出请求时间戳
            WritePropertyAndValue(output, "timestamp", logEvent.Timestamp.ToString("o"));
            output.Write(",");

            // 输出日志级别
            WritePropertyAndValue(output, "level", logEvent.Level.ToString());
            output.Write(",");

            // 输出log内容
            WritePropertyAndValue(output, "executeResult", logEvent.MessageTemplate.Render(logEvent.Properties));

            if (logEvent.Exception != null)
            {
                output.Write(",");
                WritePropertyAndValue(output, "exception", logEvent.Exception.ToString());
            }

            WriteProperties(logEvent.Properties, output);

            output.Write('}');
        }

        private static void WritePropertyAndValue(TextWriter output, string propertyKey, string propertyValue)
        {
            JsonValueFormatter.WriteQuotedJsonString(propertyKey, output);
            output.Write(":");
            JsonValueFormatter.WriteQuotedJsonString(propertyValue, output);
        }

        private static void WriteProperties(IReadOnlyDictionary<string, LogEventPropertyValue> properties, TextWriter output)
        {
            if (properties.Any())
                output.Write(",");

            var precedingDelimiter = "";
            foreach (var property in properties)
            {
                output.Write(precedingDelimiter);
                precedingDelimiter = ",";

                var camelCasePropertyKey = property.Key[0].ToString().ToLower() + property.Key.Substring(1);
                JsonValueFormatter.WriteQuotedJsonString(camelCasePropertyKey, output);
                output.Write(":");
                valueFormatter.Format(property.Value, output);
            }
        }
    }
}
