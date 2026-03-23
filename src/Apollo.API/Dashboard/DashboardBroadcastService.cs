using Apollo.Core.Dashboard;

using Microsoft.AspNetCore.SignalR;

using StackExchange.Redis;

namespace Apollo.API.Dashboard;

public sealed class DashboardBroadcastService(
  IServiceScopeFactory serviceScopeFactory,
  DashboardConnectionTracker connectionTracker,
  IConnectionMultiplexer redis,
  IHubContext<DashboardHub> hubContext,
  ILogger<DashboardBroadcastService> logger) : BackgroundService
{
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    string? lastSignature = null;
    var gate = new SemaphoreSlim(1, 1);
    ISubscriber subscriber = redis.GetSubscriber();
    ChannelMessageQueue queue = await subscriber.SubscribeAsync(RedisChannel.Literal(DashboardChannels.OverviewUpdated));

    queue.OnMessage(async _ =>
    {
      try
      {
        await gate.WaitAsync(stoppingToken);

        if (!connectionTracker.HasConnections)
        {
          lastSignature = null;
          return;
        }

        using var scope = serviceScopeFactory.CreateScope();
        var overviewService = scope.ServiceProvider.GetRequiredService<IDashboardOverviewService>();
        var overview = await overviewService.GetOverviewAsync(stoppingToken);
        var signature = overviewService.CreateSignature(overview);

        if (lastSignature is null)
        {
          lastSignature = signature;
          await hubContext.Clients.All.SendAsync("DashboardOverviewUpdated", overview, stoppingToken);
          return;
        }

        if (signature == lastSignature)
        {
          return;
        }

        lastSignature = signature;
        await hubContext.Clients.All.SendAsync("DashboardOverviewUpdated", overview, stoppingToken);
      }
      catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
      {
        // ignored
      }
      catch (Exception ex)
      {
        DashboardLogs.DashboardBroadcastFailed(logger, ex);
      }
      finally
      {
        if (gate.CurrentCount == 0)
        {
          gate.Release();
        }
      }
    });

    try
    {
      await Task.Delay(Timeout.Infinite, stoppingToken);
    }
    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
    {
      // ignored
    }
    finally
    {
      await queue.UnsubscribeAsync();
      gate.Dispose();
    }
  }
}
