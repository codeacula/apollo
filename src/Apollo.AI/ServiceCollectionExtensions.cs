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

    // Load system prompts from prompty files if not provided in config
    config = LoadPromptyFilesIfNeeded(config);

    _ = services
      .AddSingleton(config)
      .AddTransient<IApolloAIAgent, ApolloAIAgent>()
      .AddTransient<IReminderMessageGenerator, ApolloReminderMessageGenerator>();

    return services;
  }

  private static ApolloAIConfig LoadPromptyFilesIfNeeded(ApolloAIConfig config)
  {
    var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

    // Load legacy system prompt for backward compatibility
    if (string.IsNullOrWhiteSpace(config.SystemPrompt))
    {
      var promptyPath = Path.Combine(baseDirectory, "Prompts", "Apollo.prompty");
      config = LoadPromptyFile(config, promptyPath, (c, prompt) => c with { SystemPrompt = prompt }, "system prompt");
    }

    // Load tool calling system prompt
    if (string.IsNullOrWhiteSpace(config.ToolCallingSystemPrompt))
    {
      var toolCallingPath = Path.Combine(baseDirectory, "Prompts", "ApolloToolCalling.prompty");
      config = LoadPromptyFile(config, toolCallingPath, (c, prompt) => c with { ToolCallingSystemPrompt = prompt }, "tool calling prompt");
    }

    // Load response system prompt
    if (string.IsNullOrWhiteSpace(config.ResponseSystemPrompt))
    {
      var responsePath = Path.Combine(baseDirectory, "Prompts", "ApolloResponse.prompty");
      config = LoadPromptyFile(config, responsePath, (c, prompt) => c with { ResponseSystemPrompt = prompt }, "response prompt");
    }

    return config;
  }

  private static ApolloAIConfig LoadPromptyFile(
    ApolloAIConfig config,
    string promptyPath,
    Func<ApolloAIConfig, string, ApolloAIConfig> updateConfig,
    string promptName)
  {
    if (File.Exists(promptyPath))
    {
      try
      {
        var systemPrompt = PromptyLoader.LoadSystemPromptFromFile(promptyPath);
        config = updateConfig(config, systemPrompt);
        Console.WriteLine($"Loaded {promptName} from {promptyPath}");
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Warning: Failed to load {promptName} file: {ex.Message}");
      }
    }
    else
    {
      Console.WriteLine($"Warning: Prompty file not found at {promptyPath}");
    }

    return config;
  }
}
