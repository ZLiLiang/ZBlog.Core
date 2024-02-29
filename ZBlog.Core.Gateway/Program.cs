using Microsoft.AspNetCore.Authentication;
using ZBlog.Core.Common.GlobalVars;
using ZBlog.Core.Common.Helper;
using ZBlog.Core.Gateway.Extensions;
using ZBlog.Core.Extensions.ServiceExtensions;
using ZBlog.Core.Common.Caches;
using System.Reflection;
using ZBlog.Core.Gateway.Helper;

var builder = WebApplication.CreateBuilder(args);

// ÅäÖÃhostÓëÈÝÆ÷
#pragma warning disable ASP0013 // Suggest switching from using Configure methods to WebApplicationBuilder.Configuration
builder.Host.ConfigureAppConfiguration((hostingContext, config) =>
{
    config.AddJsonFile("appsettings.gw.json", optional: true, reloadOnChange: false)
        .AddJsonFile($"appsettings.gw.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: false)
        .AddJsonFile("ocelot.json", optional: true, reloadOnChange: true)
        .AddJsonFile($"ocelot.{hostingContext.HostingEnvironment.EnvironmentName}.json", true, true);
});
#pragma warning restore ASP0013 // Suggest switching from using Configure methods to WebApplicationBuilder.Configuration

builder.WebHost.UseUrls("http://*:9000");

// Add services to the container.
builder.Services.AddSingleton(new AppSettings(builder.Configuration));

builder.Services.AddAuthentication()
    .AddScheme<AuthenticationSchemeOptions, CustomAuthenticationHandler>(Permissions.GWName, _ => { });

builder.Services.AddCustomSwaggerSetup();

builder.Services.AddControllers();

builder.Services.AddHttpContextSetup();

builder.Services.AddCorsSetup();

builder.Services.AddMemoryCache();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSingleton<ICaching, Caching>();

builder.Services.AddCustomOcelotSetup();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseCustomSwaggerMildd(() => Assembly.GetExecutingAssembly().GetManifestResourceStream("ZBlog.Core.Gateway.index.html"));

app.UseCors(AppSettings.App(new string[] { "Startup", "Cors", "PolicyName" }));

#pragma warning disable ASP0014 // Suggest using top level route registrations
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});
#pragma warning restore ASP0014 // Suggest using top level route registrations

app.UseMiddleware<CustomJwtTokenAuthMiddleware>();

app.UseCustomOcelotMildd().Wait();

app.Run();