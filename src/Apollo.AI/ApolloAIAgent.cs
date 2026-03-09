using Apollo.AI.DTOs;
using Apollo.AI.Prompts;
using Apollo.AI.Requests;
using Apollo.Core.Configuration;

using Microsoft.Extensions.Logging;

namespace Apollo.AI;

public sealed class ApolloAIAgent(
  IConfigurationStore configurationStore,
  IPromptLoader promptLoader,
  IPromptTemplateProcessor templateProcessor,
  ILogger<AIRequestBuilder> logger) : IApolloAIAgent
{
  private const string ToolPlanningPromptName = "ApolloToolPlanning";
  private const string ResponsePromptName = "ApolloResponse";
  private const string ReminderPromptName = "ApolloReminder";
  private const string DailyPlanningPromptName = "ApolloDailyPlanning";
  private const string TimeParsingPromptName = "ApolloTimeParsing";

  public IAIRequestBuilder CreateRequest()
  {
    return new AIRequestBuilder(templateProcessor, logger);
  }

  private async Task<IAIRequestBuilder> CreateConfiguredRequestAsync()
  {
    var modelIdResult = await configurationStore.GetConfigurationAsync(ConfigurationKeys.AiModelId);
    var endpointResult = await configurationStore.GetConfigurationAsync(ConfigurationKeys.AiEndpoint);
    var apiKeyResult = await configurationStore.GetConfigurationAsync(ConfigurationKeys.AiApiKey);

    var modelId = modelIdResult.IsSuccess ? modelIdResult.Value.Value : "";
    var endpoint = endpointResult.IsSuccess ? endpointResult.Value.Value : "";
    var apiKey = apiKeyResult.IsSuccess ? apiKeyResult.Value.Value : "";

    return CreateRequest().WithConfig(modelId, endpoint, apiKey);
  }

  public async Task<IAIRequestBuilder> CreateToolPlanningRequestAsync(
    IEnumerable<ChatMessageDTO> messages,
    string userTimezone,
    string activeTodos)
  {
    var prompt = await promptLoader.LoadAsync(ToolPlanningPromptName);
    var currentDateTime = DateTimeOffset.Now.ToString("yyyy-MM-ddTHH:mm:sszzz", System.Globalization.CultureInfo.InvariantCulture);

    var variables = new Dictionary<string, string>
    {
      ["current_datetime"] = currentDateTime,
      ["user_timezone"] = userTimezone,
      ["active_todos"] = activeTodos
    };

    var request = await CreateConfiguredRequestAsync();
    return request
      .FromPromptDefinition(prompt)
      .WithMessages(messages)
      .WithToolCalling(enabled: false)
      .WithJsonMode(enabled: true)
      .WithTemplateVariables(variables);
  }

  public async Task<IAIRequestBuilder> CreateResponseRequestAsync(
    IEnumerable<ChatMessageDTO> messages,
    string actionsSummary,
    string userTimezone)
  {
    var prompt = await promptLoader.LoadAsync(ResponsePromptName);

    var variables = new Dictionary<string, string>
    {
      ["actions_taken"] = actionsSummary,
      ["user_timezone"] = userTimezone
    };

    var request = await CreateConfiguredRequestAsync();
    return request
      .FromPromptDefinition(prompt)
      .WithMessages(messages)
      .WithToolCalling(enabled: false)
      .WithTemplateVariables(variables);
  }

  public async Task<IAIRequestBuilder> CreateReminderRequestAsync(
    string userTimezone,
    string currentTime,
    string reminderItems)
  {
    var prompt = await promptLoader.LoadAsync(ReminderPromptName);

    var variables = new Dictionary<string, string>
    {
      ["user_timezone"] = userTimezone,
      ["current_time"] = currentTime,
      ["reminder_items"] = reminderItems
    };

    var request = await CreateConfiguredRequestAsync();
    return request
      .FromPromptDefinition(prompt)
      .WithTemplateVariables(variables);
  }

  public async Task<IAIRequestBuilder> CreateDailyPlanRequestAsync(
    string userTimezone,
    string currentTime,
    string activeTodos,
    int taskCount)
  {
    var prompt = await promptLoader.LoadAsync(DailyPlanningPromptName);

    var variables = new Dictionary<string, string>
    {
      ["user_timezone"] = userTimezone,
      ["current_time"] = currentTime,
      ["active_todos"] = activeTodos,
      ["task_count"] = taskCount.ToString(System.Globalization.CultureInfo.InvariantCulture)
    };

    var request = await CreateConfiguredRequestAsync();
    return request
      .FromPromptDefinition(prompt)
      .WithToolCalling(enabled: false)
      .WithJsonMode(enabled: true)
      .WithTemplateVariables(variables);
  }

  public async Task<IAIRequestBuilder> CreateTimeParsingRequestAsync(
    string timeExpression,
    string userTimezone,
    string currentDateTime)
  {
    var prompt = await promptLoader.LoadAsync(TimeParsingPromptName);

    var variables = new Dictionary<string, string>
    {
      ["current_datetime"] = currentDateTime,
      ["user_timezone"] = userTimezone
    };

    var request = await CreateConfiguredRequestAsync();
    return request
      .FromPromptDefinition(prompt)
      .WithMessage(new ChatMessageDTO(Enums.ChatRole.User, timeExpression, DateTime.UtcNow))
      .WithToolCalling(enabled: false)
      .WithTemplateVariables(variables);
  }
}
