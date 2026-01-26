using Apollo.Core.Notifications;
using Apollo.Domain.People.Models;

using FluentResults;

namespace Apollo.Notifications;

/// <summary>
/// A no-op implementation of <see cref="IPersonNotificationClient"/> that does nothing.
/// Used as a fallback when notification services are not configured.
/// </summary>
public sealed class NoOpPersonNotificationClient : IPersonNotificationClient
{
  public Task<Result> SendNotificationAsync(Person person, Notification notification, CancellationToken cancellationToken = default)
  {
    return Task.FromResult(Result.Ok());
  }
}
