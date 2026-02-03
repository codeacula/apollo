using Apollo.AI.DTOs;
using Apollo.AI.Requests;

namespace Apollo.AI;

public interface IApolloAIAgent
{
  /// <summary>
  /// Creates a new request builder for custom configuration.
  /// </summary>
  IAIRequestBuilder CreateRequest();

  /// <summary>
  /// Creates a request pre-configured for tool planning (low temperature, JSON output).
  /// </summary>
  /// <param name="messages"></param>
  /// <param name="userTimezone"></param>
  /// <param name="activeTodos"></param>
  IAIRequestBuilder CreateToolPlanningRequest(
    IEnumerable<ChatMessageDTO> messages,
    string userTimezone,
    string activeTodos);

  /// <summary>
  /// Creates a request pre-configured for response generation (higher temperature, no tools).
  /// </summary>
  /// <param name="messages"></param>
  /// <param name="actionsSummary"></param>
  /// <param name="userTimezone"></param>
  IAIRequestBuilder CreateResponseRequest(
    IEnumerable<ChatMessageDTO> messages,
    string actionsSummary,
    string userTimezone);

  /// <summary>
  /// Creates a request pre-configured for reminder generation.
  /// </summary>
  /// <param name="userTimezone"></param>
  /// <param name="currentTime"></param>
  /// <param name="reminderItems"></param>
  IAIRequestBuilder CreateReminderRequest(
    string userTimezone,
    string currentTime,
    string reminderItems);

  /// <summary>
  /// Creates a request pre-configured for daily planning (JSON output with task selection).
  /// </summary>
  /// <param name="userTimezone"></param>
  /// <param name="currentTime"></param>
  /// <param name="activeTodos"></param>
  /// <param name="taskCount"></param>
  IAIRequestBuilder CreateDailyPlanRequest(
    string userTimezone,
    string currentTime,
    string activeTodos,
    int taskCount);
}
