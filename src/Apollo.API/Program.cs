using Apollo.AI;
using Apollo.API;
using Apollo.Database;
using Apollo.Discord;

using NetCord.Hosting.Services;

WebApplicationBuilder webAppBuilder = WebApplication.CreateBuilder(args);
var configuration = webAppBuilder.Configuration;

_ = webAppBuilder.Services.AddControllers();
_ = webAppBuilder.Services
  .AddDatabaseServices(configuration)
  .AddAPIServices(configuration)
  .AddAiServices(configuration)
  .AddDiscordServices();

WebApplication app = webAppBuilder.Build();

// Apply database migrations
await app.Services.MigrateDatabaseAsync();

_ = app.AddModules(typeof(IApolloAPIApp).Assembly);
_ = app.UseRequestLocalization();

if (app.Environment.IsDevelopment())
{
  _ = app.MapOpenApi();
}

_ = app.MapControllers();
_ = app.UseHttpsRedirection();
_ = app.UseDefaultFiles();
_ = app.UseStaticFiles();

await app.RunAsync();
