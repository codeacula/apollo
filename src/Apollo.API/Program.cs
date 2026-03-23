using Apollo.Application.Configuration;
using Apollo.Cache;
using Apollo.Database;
using Apollo.GRPC;


WebApplicationBuilder webAppBuilder = WebApplication.CreateBuilder(args);
var configuration = webAppBuilder.Configuration;

_ = webAppBuilder.Services.AddControllers();
_ = webAppBuilder.Services.AddSignalR();
_ = webAppBuilder.Services.AddSingleton<Apollo.API.Dashboard.DashboardConnectionTracker>();
_ = webAppBuilder.Services
  .AddCacheServices(configuration.GetConnectionString("Redis")!)
  .AddGrpcClientServices()
  .AddDatabaseServices(configuration);

_ = webAppBuilder.Services
  .AddScoped<Apollo.API.Dashboard.IDashboardOverviewService, Apollo.API.Dashboard.DashboardOverviewService>()
  .AddHostedService<Apollo.API.Dashboard.DashboardBroadcastService>();

// Register MediatR scoped to only the configuration handlers.
// Apollo.API is a pure REST gateway and only needs configuration CQRS handlers.
// We register handlers explicitly to avoid pulling in AI-dependent handlers
// from the full Apollo.Application assembly scan.
_ = webAppBuilder.Services.AddMediatR(cfg =>
  cfg.RegisterServicesFromAssemblyContaining<GetInitializationStatusQueryHandler>()
);

// Disable DI validate-on-build: Apollo.API is a gateway — not all registered handlers
// (from transitive assembly scans) need resolvable dependencies in this host.
webAppBuilder.Host.UseDefaultServiceProvider(options =>
{
  options.ValidateOnBuild = false;
  options.ValidateScopes = webAppBuilder.Environment.IsDevelopment();
});

WebApplication app = webAppBuilder.Build();

_ = app.UseRequestLocalization();

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
_ = app.MapHub<Apollo.API.Dashboard.DashboardHub>("/hubs/dashboard");
_ = app.MapFallback(async context =>
{
  if (context.Request.Path.StartsWithSegments("/api") || context.Request.Path.StartsWithSegments("/hubs"))
  {
    context.Response.StatusCode = StatusCodes.Status404NotFound;
    return;
  }

  await context.Response.SendFileAsync(Path.Combine(app.Environment.WebRootPath!, "index.html"));
});

await app.RunAsync();
