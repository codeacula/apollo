using Apollo.Application.Configuration;
using Apollo.Cache;
using Apollo.API.Dashboard;
using Apollo.Core.Configuration;
using Apollo.Database;
using Apollo.GRPC;
using FluentResults;
using MediatR;


WebApplicationBuilder webAppBuilder = WebApplication.CreateBuilder(args);
var configuration = webAppBuilder.Configuration;

_ = webAppBuilder.Services.AddControllers();
_ = webAppBuilder.Services.AddSignalR();
_ = webAppBuilder.Services.AddSingleton<DashboardConnectionTracker>();

var redisConnectionString = configuration.GetConnectionString("Redis");
_ = webAppBuilder.Services
  .AddGrpcClientServices()
  .AddDatabaseServices(configuration);

if (!string.IsNullOrWhiteSpace(redisConnectionString))
{
  _ = webAppBuilder.Services
    .AddCacheServices(redisConnectionString)
    .AddHostedService<DashboardBroadcastService>();
}

_ = webAppBuilder.Services
  .AddScoped<IDashboardOverviewService, DashboardOverviewService>()
  .AddSingleton(TimeProvider.System);

// Register only the specific MediatR handlers Apollo.API needs.
// Apollo.Application lives in the same assembly as AI/scheduler-dependent handlers,
// so we cannot use RegisterServicesFromAssemblyContaining — it would pull in handlers
// whose dependencies (IApolloAIAgent, IToDoReminderScheduler) are not registered here.
_ = webAppBuilder.Services.AddMediatR(cfg =>
  cfg.RegisterServicesFromAssemblyContaining<DashboardConnectionTracker>());
_ = webAppBuilder.Services
  .AddTransient<IRequestHandler<GetInitializationStatusQuery, Result<InitializationStatus>>, GetInitializationStatusQueryHandler>()
  .AddTransient<IRequestHandler<UpdateAiConfigurationCommand, Result<ConfigurationData>>, UpdateAiConfigurationCommandHandler>()
  .AddTransient<IRequestHandler<UpdateDiscordConfigurationCommand, Result<ConfigurationData>>, UpdateDiscordConfigurationCommandHandler>()
  .AddTransient<IRequestHandler<UpdateSuperAdminConfigurationCommand, Result<ConfigurationData>>, UpdateSuperAdminConfigurationCommandHandler>();

WebApplication app = webAppBuilder.Build();

if (app.Environment.IsDevelopment())
{
  _ = app.MapOpenApi();
}

var appUrls = configuration["ASPNETCORE_URLS"];
if (!string.IsNullOrWhiteSpace(appUrls) && appUrls.Contains("https://", StringComparison.OrdinalIgnoreCase))
{
  _ = app.UseHttpsRedirection();
}
_ = app.UseDefaultFiles();
_ = app.UseStaticFiles();
_ = app.MapStaticAssets();
_ = app.MapControllers();
_ = app.MapHub<DashboardHub>("/hubs/dashboard");
#pragma warning disable ASP0018 // route parameter 'path' is intentionally unused — catch-all for SPA routing
_ = app.MapMethods("{**path}", [HttpMethods.Get, HttpMethods.Head], async context =>
#pragma warning restore ASP0018
{
  if (context.Request.Path.StartsWithSegments("/api")
    || context.Request.Path.StartsWithSegments("/hubs")
    || context.Request.Path.StartsWithSegments("/assets"))
  {
    context.Response.StatusCode = StatusCodes.Status404NotFound;
    return;
  }

  if (Path.HasExtension(context.Request.Path.Value))
  {
    context.Response.StatusCode = StatusCodes.Status404NotFound;
    return;
  }

  var webRootPath = app.Environment.WebRootPath;
  if (string.IsNullOrWhiteSpace(webRootPath))
  {
    context.Response.StatusCode = StatusCodes.Status404NotFound;
    return;
  }

  var indexPath = Path.Combine(webRootPath, "index.html");
  if (!File.Exists(indexPath))
  {
    context.Response.StatusCode = StatusCodes.Status404NotFound;
    return;
  }

  context.Response.ContentType = "text/html; charset=utf-8";
  await context.Response.SendFileAsync(indexPath);
});

await app.RunAsync();
