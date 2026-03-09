using Apollo.AI.Config;
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


    var config = apolloAiConfig ?? new ApolloAIConfig();

    _ = services
      .AddSingleton(config)
      .AddSingleton<IPromptLoader, PromptLoader>()
      .AddSingleton<IPromptTemplateProcessor, PromptTemplateProcessor>()
      .AddTransient<IApolloAIAgent, ApolloAIAgent>()
      .AddTransient<IReminderMessageGenerator, ApolloReminderMessageGenerator>();

    return services;
  }
}
