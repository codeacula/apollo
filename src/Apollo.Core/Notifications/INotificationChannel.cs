using Apollo.Domain.Common.Enums;

using FluentResults;

namespace Apollo.Core.Notifications;

/// <summary>
/// Defines a notification channel that can send notifications to a specific identifier.
/// </summary>
public interface INotificationChannel
{
  /// <summary>
  /// Gets the type of notification channel.
  /// </summary>
  NotificationChannelType ChannelType { get; }

  /// <summary>
  /// Sends a notification to the specified identifier.
  /// </summary>
  /// <param name="identifier">The channel-specific identifier (e.g., Discord user ID, email address).</param>
  /// <param name="notification">The notification to send.</param>
  /// <param name="cancellationToken">A token to cancel the operation.</param>
  /// <returns>A result indicating success or failure.</returns>
  Task<Result> SendAsync(string identifier, Notification notification, CancellationToken cancellationToken = default);
}
