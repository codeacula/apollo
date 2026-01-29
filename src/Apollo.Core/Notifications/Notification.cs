namespace Apollo.Core.Notifications;

/// <summary>
/// Represents a notification to be sent to a person.
/// </summary>
public sealed record Notification
{
  /// <summary>
  /// Gets the text content of the notification.
  /// </summary>
  public required string Content { get; init; }

  /// <summary>
  /// Gets the optional title of the notification.
  /// </summary>
  public string? Title { get; init; }

  /// <summary>
  /// Gets optional metadata for channel-specific components or formatting.
  /// </summary>
  public IDictionary<string, object>? Metadata { get; init; }
}
