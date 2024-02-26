using Castle.DynamicProxy;
using Microsoft.AspNetCore.Http;

namespace ZBlog.Core.Extensions.AOP
{
    /// <summary>
    /// 面向切面的缓存使用
    /// </summary>
    public class UserAuditAOP : CacheAOPBase
    {
        private readonly IHttpContextAccessor _contextAccessor;

        public UserAuditAOP(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        public override void Intercept(IInvocation invocation)
        {
            string userName = _contextAccessor.HttpContext?.User?.Identity?.Name;

            //对当前方法的特性验证
            if (invocation.Method.Name?.ToLower() == "add" || invocation.Method.Name?.ToLower() == "update")
            {

                if (invocation.Arguments.Length == 1)
                {
                    if (invocation.Arguments[0].GetType().IsClass)
                    {
                        dynamic argModel = invocation.Arguments[0];
                        var getType = argModel.GetType();
                        if (invocation.Method.Name?.ToLower() == "add")
                        {
                            if (getType.GetProperty("CreateBy") != null)
                            {
                                argModel.CreateBy = userName;
                            }
                            if (getType.GetProperty("CreateTime") != null)
                            {
                                argModel.bCreateTime = DateTime.Now;
                            }
                        }
                        if (getType.GetProperty("UpdateTime") != null)
                        {
                            argModel.bUpdateTime = DateTime.Now;
                        }
                        if (getType.GetProperty("ModifyBy") != null)
                        {
                            argModel.ModifyBy = userName;
                        }
                        if (getType.GetProperty("Submitter") != null)
                        {
                            argModel.Submitter = userName;
                        }

                        invocation.Arguments[0] = argModel;
                    }
                }
                invocation.Proceed();
            }
            else
            {
                invocation.Proceed();
            }
        }

    }
}
