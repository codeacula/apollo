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
    cancellationToken.ThrowIfCancellationRequested();

    try
    {
      ISubscriber subscriber = redis.GetSubscriber();
      _ = await subscriber.PublishAsync(RedisChannel.Literal(DashboardChannels.OverviewUpdated), "updated");
    }
    catch (Exception ex)
    {
      DashboardUpdateLogs.DashboardPublishFailed(logger, ex);
    }
  }
}
