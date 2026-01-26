using Apollo.AI.Config;
using Apollo.AI.DTOs;
using Apollo.AI.Prompts;
using Apollo.AI.Requests;

namespace Apollo.AI;

public sealed class ApolloAIAgent(
  ApolloAIConfig config,
  IPromptLoader promptLoader,
  IPromptTemplateProcessor templateProcessor) : IApolloAIAgent
{
  private const string ToolCallingPromptName = "ApolloToolCalling";
  private const string ResponsePromptName = "ApolloResponse";
  private const string ReminderPromptName = "ApolloReminder";

  public IAIRequestBuilder CreateRequest()
  {
    return new AIRequestBuilder(config, templateProcessor);
  }

  public IAIRequestBuilder CreateToolCallingRequest(
    IEnumerable<ChatMessageDTO> messages,
    IDictionary<string, object> plugins,
    string userTimezone,
    string activeTodos)
  {
    var prompt = promptLoader.Load(ToolCallingPromptName);
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
      .WithPlugins(plugins)
      .WithToolCalling(enabled: true)
      .WithTemplateVariables(variables);
  }

  public IAIRequestBuilder CreateResponseRequest(
    IEnumerable<ChatMessageDTO> messages,
    string actionsSummary,
    string userTimezone)
  {
    var prompt = promptLoader.Load(ResponsePromptName);

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

  public IAIRequestBuilder CreateReminderRequest(
    string userTimezone,
    string currentTime,
    string reminderItems)
  {
    var prompt = promptLoader.Load(ReminderPromptName);

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
}
