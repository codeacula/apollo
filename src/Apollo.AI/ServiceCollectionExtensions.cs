using System.Diagnostics.CodeAnalysis;

using Apollo.AI.Config;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Apollo.AI;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddAiServices(this IServiceCollection services, IConfiguration configuration)
  {
    var apolloAiConfig = configuration.GetSection(nameof(ApolloAIConfig)).Get<ApolloAIConfig>();

    if (apolloAiConfig == null)
    {
      Console.WriteLine("No AI configuration set; using default settings.");
    }

    _ = services
      .AddSingleton(apolloAiConfig ?? new ApolloAIConfig())
      .AddScoped<IApolloAIAgent, ApolloAIAgent>();

    return services;
  }
}
