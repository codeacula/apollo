using Apollo.AI.DTOs;
using Apollo.AI.Requests;

namespace Apollo.AI;

public interface IApolloAIAgent
{
  /// <summary>
  /// Creates a new request builder for custom configuration.
  /// </summary>
  IAIRequestBuilder CreateRequest();

  IAIRequestBuilder CreateToolPlanningRequest(
    IEnumerable<ChatMessageDTO> messages,
    string userTimezone,
    string activeTodos);

  IAIRequestBuilder CreateResponseRequest(
    IEnumerable<ChatMessageDTO> messages,
    string actionsSummary,
    string userTimezone);

  IAIRequestBuilder CreateReminderRequest(
    string userTimezone,
    string currentTime,
    string reminderItems);

  IAIRequestBuilder CreateDailyPlanRequest(
    string userTimezone,
    string currentTime,
    string activeTodos,
    int taskCount);

  IAIRequestBuilder CreateTimeParsingRequest(
    string timeExpression,
    string userTimezone,
    string currentDateTime);
}
