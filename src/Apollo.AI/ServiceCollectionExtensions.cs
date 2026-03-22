using Apollo.AI.Config;
using Apollo.AI.Prompts;
using Apollo.Core.ToDos;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Apollo.AI;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddAiServices(this IServiceCollection services, IConfiguration configuration)
  {
    var apolloAiConfig = configuration.GetSection(nameof(ApolloAIConfig)).Get<ApolloAIConfig>();

    var config = apolloAiConfig ?? new ApolloAIConfig();

    _ = services
      .AddLogging()
      .AddSingleton(config)
      .AddSingleton<IPromptLoader, PromptLoader>()
      .AddSingleton<IPromptTemplateProcessor, PromptTemplateProcessor>()
      .AddTransient<IApolloAIAgent, ApolloAIAgent>()
      .AddTransient<IReminderMessageGenerator, ApolloReminderMessageGenerator>();

    return services;
  }
}
