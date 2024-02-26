using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using ZBlog.Core.Common.Helper;
using ZBlog.Core.Extensions.Authorization.Policys;

namespace ZBlog.Core.Extensions.ServiceExtensions
{
    /// <summary>
    /// Ids4权限 认证服务
    /// </summary>
    public static class AuthenticationIds4Setup
    {
        public static void AddAuthenticationIds4Setup(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            // 添加Identityserver4认证
            services.AddAuthentication(o =>
            {
                o.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = nameof(ApiResponseHandler);
                o.DefaultForbidScheme = nameof(ApiResponseHandler);
            })
            .AddJwtBearer(options =>
            {
                options.Authority = AppSettings.App(new string[] { "Startup", "IdentityServer4", "AuthorizationUrl" });
                options.RequireHttpsMetadata = false;
                options.Audience = AppSettings.App(new string[] { "Startup", "IdentityServer4", "ApiName" });
            })
            .AddScheme<AuthenticationSchemeOptions, ApiResponseHandler>(nameof(ApiResponseHandler), o => { });
        }
    }
}
