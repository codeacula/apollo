using Apollo.AI.Config;
using Apollo.AI.Helpers;
using Apollo.Core.ToDos;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Apollo.AI;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddAiServices(this IServiceCollection services, IConfiguration configuration)
  {
    var apolloAiConfig = configuration.GetSection(nameof(ApolloAIConfig)).Get<ApolloAIConfig>();

    if (apolloAiConfig == null)
    {
      Console.WriteLine("No AI configuration set; using default settings.");
    }

    var config = apolloAiConfig ?? new ApolloAIConfig();

    // Load system prompt from prompty file if not provided in config
    if (string.IsNullOrWhiteSpace(config.SystemPrompt))
    {
      var promptyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Prompts", "Apollo.prompty");
      if (File.Exists(promptyPath))
      {
        try
        {
          var systemPrompt = PromptyLoader.LoadSystemPromptFromFile(promptyPath);
          config = config with { SystemPrompt = systemPrompt };
          Console.WriteLine($"Loaded system prompt from {promptyPath}");
        }
        catch (Exception ex)
        {
          Console.WriteLine($"Warning: Failed to load prompty file: {ex.Message}");
        }
      }
      else
      {
        Console.WriteLine($"Warning: Prompty file not found at {promptyPath}");
      }
    }

    _ = services
      .AddSingleton(config)
      .AddTransient<IApolloAIAgent, ApolloAIAgent>()
      .AddTransient<IReminderMessageGenerator, ApolloReminderMessageGenerator>();

    return services;
  }
}
