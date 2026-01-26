using Apollo.AI;
using Apollo.Application;
using Apollo.Cache;
using Apollo.Database;
using Apollo.GRPC;
using Apollo.Service;

WebApplicationBuilder webAppBuilder = WebApplication.CreateBuilder(args);
var configuration = webAppBuilder.Configuration;

_ = webAppBuilder.Services.AddControllers();
_ = webAppBuilder.Services
  .AddDatabaseServices(configuration)
  .AddCacheServices(configuration.GetConnectionString("Redis")!)
  .AddRequiredServices(configuration)
  .AddAiServices(configuration)
  .AddApplicationServices()
  .AddGrpcServerServices();

WebApplication app = webAppBuilder.Build();

// Apply database migrations
await app.Services.MigrateDatabaseAsync();

_ = app.UseRequestLocalization();

_ = app.MapControllers();
_ = app.AddGrpcServerServices();

await app.RunAsync();
