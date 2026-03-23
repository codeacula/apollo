using Apollo.Core.Dashboard;

using Microsoft.Extensions.Logging;

using StackExchange.Redis;

namespace Apollo.Cache;

public sealed class DashboardUpdatePublisher(
  IConnectionMultiplexer redis,
  ILogger<DashboardUpdatePublisher> logger) : IDashboardUpdatePublisher
{
  public async Task PublishOverviewUpdatedAsync(CancellationToken cancellationToken = default)
  {
    try
    {
      if (cancellationToken.IsCancellationRequested)
      {
        return;
      }

      ISubscriber subscriber = redis.GetSubscriber();
      _ = await subscriber.PublishAsync(RedisChannel.Literal(DashboardChannels.OverviewUpdated), "updated");
    }
    catch (OperationCanceledException)
    {
      // Ignore cancellations after successful writes; dashboard updates are best-effort.
    }
    catch (Exception ex)
    {
      DashboardUpdateLogs.DashboardPublishFailed(logger, ex);
    }
  }
}
