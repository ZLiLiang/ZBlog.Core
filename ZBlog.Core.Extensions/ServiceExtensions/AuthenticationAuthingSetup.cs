using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NetDevPack.Security.JwtExtensions;
using ZBlog.Core.Common.Helper;
using ZBlog.Core.Extensions.Authorization.Policys;

namespace ZBlog.Core.Extensions.ServiceExtensions
{
    /// <summary>
    /// Authing权限 认证服务
    /// </summary>
    public static class AuthenticationAuthingSetup
    {
        public static void AddAuthenticationAuthingSetup(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            TokenValidationParameters tokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = AppSettings.App(new string[] { "Startup", "Authing", "Issuer" }),
                ValidAudience = AppSettings.App(new string[] { "Startup", "Authing", "Audience" }),
                ValidAlgorithms = new string[] { "RS256" }
            };

            services.AddAuthentication(o =>
            {
                //认证middleware配置
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = nameof(ApiResponseHandler);
                o.DefaultForbidScheme = nameof(ApiResponseHandler);
            })
            .AddJwtBearer(o =>
            {
                //主要是jwt  token参数设置
                o.TokenValidationParameters = tokenValidationParameters;
                o.RequireHttpsMetadata = false;
                o.SaveToken = false;
                o.IncludeErrorDetails = true;
                o.SetJwksOptions(new JwkOptions(AppSettings.App(new string[] { "Startup", "Authing", "JwksUri" }), AppSettings.App(new string[] { "Startup", "Authing", "Issuer" }), new TimeSpan(TimeSpan.TicksPerDay)));
            })
            .AddScheme<AuthenticationSchemeOptions, ApiResponseHandler>(nameof(ApiResponseHandler), o => { });
        }
    }
}
