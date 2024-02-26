using System.Reflection;
using Castle.DynamicProxy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using StackExchange.Profiling;
using ZBlog.Core.Common.Helper;
using ZBlog.Core.Common.Hubs;
using ZBlog.Core.Common.LogHelper;

namespace ZBlog.Core.Extensions.AOP
{
    /// <summary>
    /// 拦截器LogAOP 继承IInterceptor接口
    /// </summary>
    public class LogAOP : IInterceptor
    {
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IHttpContextAccessor _accessor;

        public LogAOP(IHubContext<ChatHub> hubContext, IHttpContextAccessor accessor)
        {
            _hubContext = hubContext;
            _accessor = accessor;
        }

        /// <summary>
        /// 实例化IInterceptor唯一方法
        /// </summary>
        /// <param name="invocation"></param>
        public void Intercept(IInvocation invocation)
        {
            string userName = _accessor.HttpContext?.User?.Identity?.Name;
            string json = string.Empty;
            try
            {
                json = JsonConvert.SerializeObject(invocation.Arguments);
            }
            catch (Exception ex)
            {
                json = "无法序列化，可能是兰姆达表达式等原因造成，按照框架优化代码" + ex.ToString();
            }

            DateTime startTime = DateTime.Now;
            AOPLogInfo apiLogAopInfo = new AOPLogInfo
            {
                RequestTime = startTime.ToString("yyyy-MM-dd hh:mm:ss fff"),
                OpUserName = userName,
                RequestMethodName = invocation.Method.Name,
                RequestParamsName = string.Join(",", invocation.Arguments.Select(it => (it ?? "").ToString()).ToArray()),
                ResponseJsonData = json
            };

            try
            {
                MiniProfiler.Current.Step($"执行Service方法：{invocation.Method.Name}() -> ");
                //在被拦截的方法执行完毕后 继续执行当前方法，注意是被拦截的是异步的
                invocation.Proceed();

                // 异步获取异常，先执行
                if (IsAsyncMethod(invocation.Method))
                {
                    //Wait task execution and modify return value
                    if (invocation.Method.ReflectedType == typeof(Task))
                    {
                        invocation.ReturnValue = InternalAsyncHelper.AwaitTaskWithPostActionAndFinally((Task)invocation.ReturnValue,
                            async () => await SuccessAction(invocation, apiLogAopInfo, startTime),/*成功时执行*/
                            ex =>
                            {
                                LogEx(ex, apiLogAopInfo);
                            });
                    }
                    else
                    {
                        invocation.ReturnValue = InternalAsyncHelper.CallAwaitTaskWithPostActionAndFinallyAndGetResult(invocation.Method.ReturnType.GenericTypeArguments[0],
                            invocation.ReturnValue,
                            async o => await SuccessAction(invocation, apiLogAopInfo, startTime, o), /*成功时执行*/
                            ex =>
                            {
                                LogEx(ex, apiLogAopInfo);
                            });
                    }
                }
                else
                {
                    // 同步1
                    string jsonResult = string.Empty;
                    try
                    {
                        jsonResult = JsonConvert.SerializeObject(invocation.ReturnValue);
                    }
                    catch (Exception ex)
                    {
                        jsonResult = $"无法序列化，可能是兰姆达表达式等原因造成，按照框架优化代码{ex.ToString()}";
                    }

                    var type = invocation.Method.ReturnType;
                    var resultProperty = type.GetProperty("Result");
                    DateTime endTime = DateTime.Now;
                    string responseTime = (endTime - startTime).Milliseconds.ToString();

                    apiLogAopInfo.ResponseTime = endTime.ToString("yyyy-MM-dd hh:mm:ss fff");
                    apiLogAopInfo.ResponseIntervalTime = responseTime + "ms";
                    apiLogAopInfo.ResponseJsonData = jsonResult;

                    Parallel.For(0, 1, e =>
                    {
                        LogLock.OutLogAOP("AOPLog",
                            _accessor.HttpContext?.TraceIdentifier,
                            new string[] { apiLogAopInfo.GetType().ToString(), JsonConvert.SerializeObject(apiLogAopInfo) });
                    });
                }
            }
            catch (Exception ex)// 同步2
            {
                LogEx(ex, apiLogAopInfo);
                throw;
            }

            if (AppSettings.App(new string[] { "Middleware", "SignalRSendLog", "Enabled" }).ObjToBool())
                _hubContext.Clients.All.SendAsync("ReceiveUpdate", LogLock.GetLogData()).Wait();
        }

        private async Task SuccessAction(IInvocation invocation,
            AOPLogInfo apiLogAopInfo,
            DateTime startTime,
            object o = null)
        {
            DateTime endTime = DateTime.Now;
            string responseTime = (endTime - startTime).Milliseconds.ToString();
            apiLogAopInfo.ResponseTime = endTime.ToString("yyyy-MM-dd hh:mm:ss fff");
            apiLogAopInfo.ResponseIntervalTime = responseTime + "ms";
            apiLogAopInfo.ResponseJsonData = JsonConvert.SerializeObject(o);

            await Task.Run(() =>
            {
                Parallel.For(0, 1, e =>
                {
                    LogLock.OutLogAOP("AOPLog",
                        _accessor.HttpContext?.TraceIdentifier,
                        new string[] { apiLogAopInfo.GetType().ToString(), JsonConvert.SerializeObject(apiLogAopInfo) });
                });
            });
        }

        private void LogEx(Exception ex, AOPLogInfo dataIntercept)
        {
            if (ex != null)
            {
                //执行的 service 中，收录异常
                MiniProfiler.Current.CustomTiming("Errors：", ex.Message);
                //执行的 service 中，捕获异常
                AOPLogExInfo apiLogAopExInfo = new AOPLogExInfo
                {
                    ExMessage = ex.Message,
                    InnerException = "InnerException-内部异常:\r\n" + (ex.InnerException == null ? "" : ex.InnerException.InnerException.ToString()) +
                                     ("\r\nStackTrace-堆栈跟踪:\r\n") + (ex.StackTrace == null ? "" : ex.StackTrace.ToString()),
                    ApiLogAopInfo = dataIntercept
                };

                // 异常日志里有详细的堆栈信息
                Parallel.For(0, 1, e =>
                {
                    LogLock.OutLogAOP("AOPLogEx",
                        _accessor.HttpContext?.TraceIdentifier,
                        new string[] { apiLogAopExInfo.GetType().ToString(), JsonConvert.SerializeObject(apiLogAopExInfo) });
                });
            }
        }

        public static bool IsAsyncMethod(MethodInfo method)
        {
            return (method.ReturnType == typeof(Task) ||
                (method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task)));
        }
    }

    internal static class InternalAsyncHelper
    {
        public static async Task AwaitTaskWithPostActionAndFinally(Task actualReturnValue,
            Func<Task> postAction,
            Action<Exception> finalAction)
        {
            Exception exception = null;

            try
            {
                await actualReturnValue;
                await postAction();
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            finally
            {
                finalAction(exception);
            }
        }

        public static async Task<T> AwaitTaskWithPostActionAndFinallyAndGetResult<T>(Task<T> actualReturnValue,
            Func<object, Task> postAction,
            Action<Exception> finalAction)
        {
            Exception exception = null;

            try
            {
                var result = await actualReturnValue;
                await postAction(result);

                return result;
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                finalAction(exception);
            }
        }

        public static object CallAwaitTaskWithPostActionAndFinallyAndGetResult(Type taskReturnType,
            object actualReturnValue,
            Func<object, Task> action,
            Action<Exception> finalAction)
        {
            return typeof(InternalAsyncHelper)
                .GetMethod("AwaitTaskWithPostActionAndFinallyAndGetResult", BindingFlags.Public | BindingFlags.Static)
                .MakeGenericMethod(taskReturnType)
                .Invoke(null, new object[] { actualReturnValue, action, finalAction });
        }
    }
}
