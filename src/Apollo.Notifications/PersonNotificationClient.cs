using Apollo.Core.Notifications;
using Apollo.Domain.People.Models;
using Apollo.Domain.People.ValueObjects;

using FluentResults;

namespace Apollo.Notifications;

public sealed class PersonNotificationClient(IEnumerable<INotificationChannel> notificationChannels) : IPersonNotificationClient
{
  public async Task<Result> SendNotificationAsync(Person person, Notification notification, CancellationToken cancellationToken = default)
  {
    var enabledChannels = person.NotificationChannels.Where(c => c.IsEnabled).ToList();

    if (enabledChannels is [])
    {
      return Result.Fail("Person has no enabled notification channels");
    }

    var results = await SendToEnabledChannelsAsync(enabledChannels, notification, cancellationToken);

    return EvaluateNotificationResults(results);
  }

  private async Task<List<Result>> SendToEnabledChannelsAsync(List<NotificationChannel> enabledChannels, Notification notification, CancellationToken cancellationToken)
  {
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

    return results;
  }

  private Result EvaluateNotificationResults(List<Result> results)
  {
    var successCount = results.Count(r => r.IsSuccess);

    return (successCount, results.Count) switch
    {
      (0, _) => Result.Fail($"Failed to send notification to any channel. Errors: {string.Join("; ", results.SelectMany(r => r.Errors).Select(e => e.Message))}"),
      (var s, var t) when s < t => Result.Ok().WithSuccess($"Sent to {s}/{t} channels. Partial failures: {string.Join("; ", results.Where(r => r.IsFailed).SelectMany(r => r.Errors).Select(e => e.Message))}"),
      _ => Result.Ok()
    };
  }
}
