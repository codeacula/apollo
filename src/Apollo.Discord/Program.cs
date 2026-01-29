using Apollo.Cache;
using Apollo.Discord;
using Apollo.GRPC;

using NetCord.Hosting.AspNetCore;
using NetCord.Hosting.Services;

var builder = WebApplication.CreateBuilder(args);

_ = builder.Configuration.AddEnvironmentVariables()
      .AddUserSecrets<IApolloDiscord>();

var redisConnection = builder.Configuration.GetConnectionString("Redis")
  ?? throw new InvalidOperationException("Redis connection string not found");

// Add services to the container.
_ = builder.Services
  .AddCacheServices(redisConnection)
      .AddGrpcClientServices()
      .AddDiscordServices();

var app = builder.Build();

_ = app.AddModules(typeof(IApolloDiscord).Assembly);
_ = app.UseHttpInteractions("/interactions");
_ = app.UseRequestLocalization();

await app.RunAsync();
