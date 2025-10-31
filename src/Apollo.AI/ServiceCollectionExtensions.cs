using Apollo.AI.Config;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Apollo.AI;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddAiServices(this IServiceCollection services, IConfiguration configuration)
  {
    var apolloAiConfig = configuration.GetSection(nameof(ApolloAIConfig)).Get<ApolloAIConfig>() ?? new ApolloAIConfig();

    _ = services
      .AddSingleton(apolloAiConfig);
    _ = services.AddSingleton<IApolloAIAgent, ApolloAIAgent>();

    return services;
  }
}
