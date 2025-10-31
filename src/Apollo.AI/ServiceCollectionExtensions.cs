using Apollo.AI.Config;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Apollo.AI;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddAiServices(this IServiceCollection services, IConfiguration configuration)
  {
    _ = services.Configure<ApolloAIConfig>(configuration.GetSection(nameof(ApolloAIConfig)));

    _ = services.AddTransient<IApolloAIAgent, ApolloAIAgent>();

    return services;
  }
}
