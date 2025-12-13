using System.Diagnostics.CodeAnalysis;

using Apollo.Core.People;

using Microsoft.Extensions.DependencyInjection;

using StackExchange.Redis;

namespace Apollo.Cache;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddCacheServices(this IServiceCollection services, string redisConnectionString)
  {
    _ = services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnectionString));
    _ = services.AddSingleton<IPersonCache, PersonCache>();

    return services;
  }
}
