using Apollo.AI.Config;
using Apollo.AI.DTOs;
using Apollo.AI.Prompts;
using Apollo.AI.Requests;

using Microsoft.Extensions.Logging;

namespace Apollo.AI;

public sealed class ApolloAIAgent(
  ApolloAIConfig config,
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
    return new AIRequestBuilder(config, templateProcessor, logger);
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

    return CreateRequest()
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

    return CreateRequest()
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

    return CreateRequest()
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

    return CreateRequest()
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

    return CreateRequest()
      .FromPromptDefinition(prompt)
      .WithMessage(new ChatMessageDTO(Enums.ChatRole.User, timeExpression, DateTime.UtcNow))
      .WithToolCalling(enabled: false)
      .WithTemplateVariables(variables);
  }
}
