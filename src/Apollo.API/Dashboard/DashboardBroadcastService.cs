using Apollo.Core.Dashboard;

using Microsoft.AspNetCore.SignalR;

using StackExchange.Redis;

using System.Threading.Channels;

namespace Apollo.API.Dashboard;

public sealed class DashboardBroadcastService(
  IServiceProvider serviceProvider,
  IServiceScopeFactory serviceScopeFactory,
  DashboardConnectionTracker connectionTracker,
  IHubContext<DashboardHub> hubContext,
  ILogger<DashboardBroadcastService> logger) : BackgroundService
{
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    string? lastSignature = null;
    ChannelMessageQueue queue;

    try
    {
      var redis = serviceProvider.GetRequiredService<IConnectionMultiplexer>();
      ISubscriber subscriber = redis.GetSubscriber();
      queue = await subscriber.SubscribeAsync(RedisChannel.Literal(DashboardChannels.OverviewUpdated));
    }
    catch (Exception ex)
    {
      DashboardLogs.DashboardBroadcastDisabled(logger, ex);
      return;
    }

    var updates = Channel.CreateBounded<bool>(new BoundedChannelOptions(1)
    {
      SingleReader = true,
      SingleWriter = false,
      FullMode = BoundedChannelFullMode.DropNewest,
    });

    queue.OnMessage(_ => updates.Writer.TryWrite(true));

    try
    {
      await foreach (var _ in updates.Reader.ReadAllAsync(stoppingToken))
      {

        try
        {
          if (!connectionTracker.HasConnections)
          {
            lastSignature = null;
            continue;
          }

          using var scope = serviceScopeFactory.CreateScope();
          var overviewService = scope.ServiceProvider.GetRequiredService<IDashboardOverviewService>();
          var overview = await overviewService.GetOverviewAsync(stoppingToken);
          var signature = overviewService.CreateSignature(overview);

          if (signature == lastSignature)
          {
            continue;
          }

          lastSignature = signature;
          await hubContext.Clients.All.SendAsync("DashboardOverviewUpdated", overview, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
          break;
        }
        catch (Exception ex)
        {
          DashboardLogs.DashboardBroadcastFailed(logger, ex);
        }
      }
    }
    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
    {
      // ignored
    }
    finally
    {
      _ = updates.Writer.TryComplete();

      try
      {
        await queue.UnsubscribeAsync();
      }
      catch
      {
        // Best-effort cleanup; the host is shutting down.
      }
    }
  }
}
