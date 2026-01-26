using Apollo.Domain.Common.Enums;

namespace Apollo.Domain.People.ValueObjects;

/// <summary>
/// Represents a notification channel for a person.
/// </summary>
public readonly record struct NotificationChannel
{
  /// <summary>
  /// Gets the type of notification channel.
  /// </summary>
  public NotificationChannelType Type { get; init; }

  /// <summary>
  /// Gets the channel-specific identifier (e.g., Discord user ID, email address, push token).
  /// </summary>
  public string Identifier { get; init; }

  /// <summary>
  /// Gets a value indicating whether this channel is enabled for notifications.
  /// </summary>
  public bool IsEnabled { get; init; }

  /// <summary>
  /// Initializes a new instance of the <see cref="NotificationChannel"/> struct.
  /// </summary>
  /// <param name="type">The type of notification channel.</param>
  /// <param name="identifier">The channel-specific identifier (e.g., Discord user ID, email address, push token).</param>
  /// <param name="isEnabled">A value indicating whether this channel is enabled for notifications.</param>
  public NotificationChannel(NotificationChannelType type, string identifier, bool isEnabled = true)
  {
    Type = type;
    Identifier = identifier;
    IsEnabled = isEnabled;
  }

  /// <summary>
  /// Gets a value indicating whether this notification channel is valid.
  /// </summary>
  public bool IsValid => !string.IsNullOrWhiteSpace(Identifier);
}
