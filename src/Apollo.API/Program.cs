using Apollo.Application.Configuration;
using Apollo.Cache;
using Apollo.API.Dashboard;
using Apollo.Database;
using Apollo.GRPC;


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

// Register MediatR scoped to only the configuration handlers.
// Apollo.API is a pure REST gateway and only needs configuration CQRS handlers.
// We register handlers explicitly to avoid pulling in AI-dependent handlers
// from the full Apollo.Application assembly scan.
_ = webAppBuilder.Services.AddMediatR(cfg =>
  cfg.RegisterServicesFromAssemblyContaining<GetInitializationStatusQueryHandler>()
);

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

  await context.Response.SendFileAsync(indexPath);
});

await app.RunAsync();
