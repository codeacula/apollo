using System.Diagnostics.CodeAnalysis;

using Apollo.Application;
using Apollo.Cache;
using Apollo.GRPC;

using NetCord.Hosting.AspNetCore;
using NetCord.Hosting.Services;

namespace Apollo.Discord;

[ExcludeFromCodeCoverage]
internal static class Program
{
  private static async Task Main(string[] args)
  {
    var builder = WebApplication.CreateBuilder(args);

    _ = builder.Configuration.AddEnvironmentVariables()
      .AddUserSecrets<IApolloDiscord>();

    var redisConnection = builder.Configuration.GetConnectionString("Redis")
      ?? throw new InvalidOperationException("Redis connection string not found");

    // Add services to the container.
    _ = builder.Services
      .AddCacheServices(redisConnection)
      .AddApplicationServices()
      .AddGrpcClientServices()
      .AddDiscordServices();

    var app = builder.Build();

    _ = app.AddModules(typeof(IApolloDiscord).Assembly);
    _ = app.UseHttpInteractions("/interactions");
    _ = app.UseRequestLocalization();

    await app.RunAsync();
  }
}
