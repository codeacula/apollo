using Apollo.AI.Prompts;
using Apollo.Core.ToDos;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Apollo.AI;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddAiServices(this IServiceCollection services, IConfiguration configuration)
  {
    _ = services
      .AddSingleton<IPromptLoader, PromptLoader>()
      .AddSingleton<IPromptTemplateProcessor, PromptTemplateProcessor>()
      .AddTransient<IApolloAIAgent, ApolloAIAgent>()
      .AddTransient<IReminderMessageGenerator, ApolloReminderMessageGenerator>();

    return services;
  }
}
