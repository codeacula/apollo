using Apollo.Core.Dashboard;

using StackExchange.Redis;

namespace Apollo.Cache;

public sealed class DashboardUpdatePublisher(IConnectionMultiplexer redis) : IDashboardUpdatePublisher
{
  public async Task PublishOverviewUpdatedAsync(CancellationToken cancellationToken = default)
  {
    cancellationToken.ThrowIfCancellationRequested();

    ISubscriber subscriber = redis.GetSubscriber();
    _ = await subscriber.PublishAsync(RedisChannel.Literal(DashboardChannels.OverviewUpdated), "updated");
  }
}
