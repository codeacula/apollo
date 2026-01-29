using Apollo.Core.Notifications;
using Apollo.Domain.People.Models;

using FluentResults;

namespace Apollo.Notifications;

public sealed class PersonNotificationClient(IEnumerable<INotificationChannel> notificationChannels) : IPersonNotificationClient
{
  public async Task<Result> SendNotificationAsync(Person person, Notification notification, CancellationToken cancellationToken = default)
  {
    var enabledChannels = person.NotificationChannels.Where(c => c.IsEnabled).ToList();

    if (enabledChannels.Count == 0)
    {
      return Result.Fail("Person has no enabled notification channels");
    }

    var results = new List<Result>();

    foreach (var channel in enabledChannels)
    {
      var notificationChannel = notificationChannels.FirstOrDefault(nc => nc.ChannelType == channel.Type);
      if (notificationChannel is null)
      {
        results.Add(Result.Fail($"No notification channel implementation found for type: {channel.Type}"));
        continue;
      }

      var result = await notificationChannel.SendAsync(channel.Identifier, notification, cancellationToken);
      results.Add(result);
    }

    var successCount = results.Count(r => r.IsSuccess);

    if (successCount == 0)
    {
      return Result.Fail($"Failed to send notification to any channel. Errors: {string.Join("; ", results.SelectMany(r => r.Errors).Select(e => e.Message))}");
    }

    if (successCount < results.Count)
    {
      var errors = string.Join("; ", results.Where(r => r.IsFailed).SelectMany(r => r.Errors).Select(e => e.Message));
      return Result.Ok().WithSuccess($"Sent to {successCount}/{results.Count} channels. Partial failures: {errors}");
    }

    return Result.Ok();
  }
}
