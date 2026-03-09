using Apollo.Core.Configuration;

using NetCord.Gateway;

using StackExchange.Redis;

namespace Apollo.Discord;

/// <summary>
/// Background service that subscribes to the Redis apollo:config:updates channel
/// and reacts to Discord token changes by reconnecting the gateway client.
/// </summary>
public sealed class ConfigurationSubscriber(
  IConnectionMultiplexer redis,
  GatewayClient gatewayClient,
  ILogger<ConfigurationSubscriber> logger) : BackgroundService
{
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    var subscriber = redis.GetSubscriber();
    await subscriber.SubscribeAsync(
      RedisChannel.Literal("apollo:config:updates"),
      (_, message) => HandleMessage(message));

    logger.LogInformation("Subscribed to apollo:config:updates channel.");
    await Task.Delay(Timeout.Infinite, stoppingToken);
  }

  private void HandleMessage(RedisValue message)
  {
    if (!message.HasValue)
    {
      return;
    }

    var msg = message.ToString();

    if (msg == $"SET:{ConfigurationKeys.DiscordToken}")
    {
      logger.LogInformation("Discord token updated — reconnecting gateway.");
      _ = Task.Run(async () =>
      {
        try
        {
          await gatewayClient.CloseAsync();
          await gatewayClient.StartAsync();
        }
        catch (Exception ex)
        {
          logger.LogError(ex, "Failed to reconnect Discord gateway after token update.");
        }
      });
    }
  }
}
