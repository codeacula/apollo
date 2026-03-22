using Apollo.AI;
using Apollo.AI.Config;
using Apollo.Application;
using Apollo.Cache;
using Apollo.Core.Logging;
using Apollo.Database;
using Apollo.GRPC;
using Apollo.Service;

using Microsoft.Extensions.Logging;

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

// Check if AI is configured and log warning if not
var aiConfig = app.Services.GetRequiredService<ApolloAIConfig>();
var logger = app.Services.GetRequiredService<ILogger<Program>>();
if (string.IsNullOrWhiteSpace(aiConfig.ModelId) || string.IsNullOrWhiteSpace(aiConfig.Endpoint))
{
  AILogs.AINotConfigured(logger);
}

// Apply database migrations
await app.Services.MigrateDatabaseAsync();

_ = app.UseRequestLocalization();

_ = app.MapControllers();
_ = app.AddGrpcServerServices();

await app.RunAsync();

