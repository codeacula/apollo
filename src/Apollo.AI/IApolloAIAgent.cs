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
  /// Creates a request pre-configured for tool calling (low temperature, tools enabled).
  /// </summary>
  /// <param name="messages"></param>
  /// <param name="plugins"></param>
  /// <param name="userTimezone"></param>
  /// <param name="activeTodos"></param>
  IAIRequestBuilder CreateToolCallingRequest(
    IEnumerable<ChatMessageDTO> messages,
    IDictionary<string, object> plugins,
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
}
