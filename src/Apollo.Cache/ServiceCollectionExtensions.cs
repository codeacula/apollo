using Apollo.Core.Dashboard;
using Apollo.Core.People;

using Microsoft.Extensions.DependencyInjection;

using StackExchange.Redis;

namespace Apollo.Cache;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddCacheServices(this IServiceCollection services, string redisConnectionString)
  {
    _ = services.AddSingleton<IConnectionMultiplexer>(_ =>
    {
      var options = ConfigurationOptions.Parse(redisConnectionString);
      options.AbortOnConnectFail = false;
      return ConnectionMultiplexer.Connect(options);
    });
    _ = services.AddSingleton<IPersonCache, PersonCache>();
    _ = services.AddSingleton<IDashboardUpdatePublisher, DashboardUpdatePublisher>();

    return services;
  }
}
