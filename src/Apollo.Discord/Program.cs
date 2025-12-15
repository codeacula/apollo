using System.Diagnostics.CodeAnalysis;

using Apollo.Application;
using Apollo.Cache;
using Apollo.Discord;
using Apollo.GRPC;

using NetCord.Hosting.AspNetCore;
using NetCord.Hosting.Services;

[ExcludeFromCodeCoverage]
internal static class Program
{
  private static async Task Main(string[] args)
  {
    var builder = WebApplication.CreateBuilder(args);

    builder.Configuration.AddEnvironmentVariables()
      .AddUserSecrets<IApolloDiscord>();

    var redisConnection = builder.Configuration.GetConnectionString("Redis")
      ?? throw new InvalidOperationException("Redis connection string not found");

    // Add services to the container.
    builder.Services
      .AddCacheServices(redisConnection)
      .AddApplicationServices()
      .AddGrpcClientServices()
      .AddDiscordServices();

    var app = builder.Build();

    app.AddModules(typeof(IApolloDiscord).Assembly);
    app.UseHttpInteractions("/interactions");
    app.UseRequestLocalization();

    await app.RunAsync();
  }
}
