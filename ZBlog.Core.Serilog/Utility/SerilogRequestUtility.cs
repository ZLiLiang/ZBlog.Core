using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Events;
using ZBlog.Core.Common.Extensions;
using ZBlog.Core.Common.Helper;
using ZBlog.Core.Common.Https;

namespace ZBlog.Core.Serilog.Utility
{
    public class SerilogRequestUtility
    {
        public const string HttpMessageTemplate = "HTTP {RequestMethod} {RequestPath} QueryString:{QueryString} Body:{Body}  responded {StatusCode} in {Elapsed:0.0000} ms";

        private static readonly List<string> _ignoreUrl = new()
        {
            "/job"
        };

        private static LogEventLevel DefaultGetLevel(HttpContext context, double _, Exception? exception)
        {
            return exception is null && context.Response.StatusCode <= 499 ? LogEventLevel.Information : LogEventLevel.Error;
        }

        public static LogEventLevel GetRequestLevel(HttpContext context, double _, Exception? exception)
        {
            return exception is null && context.Response.StatusCode <= 499 ? IgnoreRequest(context) : LogEventLevel.Error;
        }

        private static LogEventLevel IgnoreRequest(HttpContext context)
        {
            var path = context.Request.Path.Value;
            if (path.IsNullOrEmpty())
                return LogEventLevel.Information;

            return _ignoreUrl.Any(s => path.StartsWith(s)) ? LogEventLevel.Verbose : LogEventLevel.Information;
        }

        /// <summary>
        /// 从Request中增加附属属性
        /// </summary>
        /// <param name="diagnosticContext"></param>
        /// <param name="httpContext"></param>
        public static void EnrichFromRequest(IDiagnosticContext diagnosticContext, HttpContext httpContext)
        {
            var request = httpContext.Request;

            diagnosticContext.Set("RequestHost", request.Host);
            diagnosticContext.Set("RequestScheme", request.Scheme);
            diagnosticContext.Set("Protocol", request.Protocol);
            diagnosticContext.Set("RequestIp", httpContext.GetRequestIp());

            if (request.Method == HttpMethods.Get)
            {
                diagnosticContext.Set("QueryString", request.QueryString.HasValue ? request.QueryString.Value : string.Empty);
                diagnosticContext.Set("Body", string.Empty);
            }
            else
            {
                diagnosticContext.Set("QueryString", request.QueryString.HasValue ? request.QueryString.Value : string.Empty);
                diagnosticContext.Set("Body", request.ContentLength > 0 ? request.GetRequestBody() : string.Empty);
            }

            diagnosticContext.Set("ContentType", httpContext.Response.ContentType);

            var endpoint = httpContext.GetEndpoint();
            if (endpoint != null)
                diagnosticContext.Set("EndpointName", endpoint.DisplayName);
        }
    }
}
