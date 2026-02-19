using Apollo.AI.DTOs;
using Apollo.AI.Requests;

namespace Apollo.AI;

public interface IApolloAIAgent
{
  /// <summary>
  /// Creates a new request builder for custom configuration.
  /// </summary>
  IAIRequestBuilder CreateRequest();

  Task<IAIRequestBuilder> CreateToolPlanningRequestAsync(
    IEnumerable<ChatMessageDTO> messages,
    string userTimezone,
    string activeTodos);

  Task<IAIRequestBuilder> CreateResponseRequestAsync(
    IEnumerable<ChatMessageDTO> messages,
    string actionsSummary,
    string userTimezone);

  Task<IAIRequestBuilder> CreateReminderRequestAsync(
    string userTimezone,
    string currentTime,
    string reminderItems);

  Task<IAIRequestBuilder> CreateDailyPlanRequestAsync(
    string userTimezone,
    string currentTime,
    string activeTodos,
    int taskCount);

  Task<IAIRequestBuilder> CreateTimeParsingRequestAsync(
    string timeExpression,
    string userTimezone,
    string currentDateTime);
}
