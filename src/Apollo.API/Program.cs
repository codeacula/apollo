using Apollo.AI;
using Apollo.API;
using Apollo.Application;
using Apollo.Database;
using Apollo.GRPC;

WebApplicationBuilder webAppBuilder = WebApplication.CreateBuilder(args);
var configuration = webAppBuilder.Configuration;

_ = webAppBuilder.Services.AddControllers();
_ = webAppBuilder.Services
  .AddDatabaseServices(configuration)
  .AddAPIServices(configuration)
  .AddAiServices(configuration)
  .AddApplicationServices()
  .AddGrpcServerServices();

WebApplication app = webAppBuilder.Build();

// Apply database migrations
await app.Services.MigrateDatabaseAsync();

_ = app.UseRequestLocalization();

if (app.Environment.IsDevelopment())
{
  _ = app.MapOpenApi();
}

_ = app.MapControllers();
_ = app.UseHttpsRedirection();
_ = app.UseDefaultFiles();
_ = app.UseStaticFiles();

_ = app.AddGrpcServerServices();

await app.RunAsync();
