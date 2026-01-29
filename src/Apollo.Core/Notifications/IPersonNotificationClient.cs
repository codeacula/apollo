using Apollo.Domain.People.Models;

using FluentResults;

namespace Apollo.Core.Notifications;

/// <summary>
/// Defines a client for sending notifications to a person across all their configured channels.
/// </summary>
public interface IPersonNotificationClient
{
  /// <summary>
  /// Sends a notification to all enabled notification channels for the specified person.
  /// </summary>
  /// <param name="person">The person to notify.</param>
  /// <param name="notification">The notification to send.</param>
  /// <param name="cancellationToken">A token to cancel the operation.</param>
  /// <returns>A result indicating success or failure. Success if at least one channel succeeded.</returns>
  Task<Result> SendNotificationAsync(Person person, Notification notification, CancellationToken cancellationToken = default);
}
