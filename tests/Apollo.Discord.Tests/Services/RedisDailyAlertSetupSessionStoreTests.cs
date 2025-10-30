using Apollo.Discord.Services;

using Microsoft.Extensions.Logging;

using Moq;

using StackExchange.Redis;

namespace Apollo.Discord.Tests.Services;

public class RedisDailyAlertSetupSessionStoreTests
{
  private readonly Mock<IConnectionMultiplexer> _mockRedis;
  private readonly Mock<IDatabase> _mockDatabase;
  private readonly Mock<ILogger<RedisDailyAlertSetupSessionStore>> _mockLogger;
  private readonly RedisDailyAlertSetupSessionStore _store;

  public RedisDailyAlertSetupSessionStoreTests()
  {
    _mockRedis = new Mock<IConnectionMultiplexer>();
    _mockDatabase = new Mock<IDatabase>();
    _mockLogger = new Mock<ILogger<RedisDailyAlertSetupSessionStore>>();

    _ = _mockRedis.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
        .Returns(_mockDatabase.Object);

    _store = new RedisDailyAlertSetupSessionStore(_mockRedis.Object, _mockLogger.Object);
  }

  [Fact]
  public async Task GetSessionAsyncNoSessionReturnsNullAsync()
  {
    _ = _mockDatabase.Setup(d => d.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
        .ReturnsAsync(RedisValue.Null);

    DailyAlertSetupSession? result = await _store.GetSessionAsync(123UL, 456UL);

    Assert.Null(result);
  }

  [Fact]
  public async Task SetSessionAsyncCallsRedisWithCorrectTtlAsync()
  {
    DailyAlertSetupSession session = new()
    {
      ChannelId = 123UL,
      RoleId = 456UL,
      Time = "08:00",
      Message = "Test"
    };

    _ = _mockDatabase.Setup(d => d.StringSetAsync(
        It.IsAny<RedisKey>(),
        It.IsAny<RedisValue>(),
        It.Is<TimeSpan?>(t => t.HasValue && t.Value.TotalMinutes == 30),
        It.IsAny<bool>(),
        It.IsAny<When>(),
        It.IsAny<CommandFlags>()))
        .ReturnsAsync(true);

    await _store.SetSessionAsync(123UL, 456UL, session);

    _mockDatabase.Verify(d => d.StringSetAsync(
        It.IsAny<RedisKey>(),
        It.IsAny<RedisValue>(),
        It.Is<TimeSpan?>(t => t.HasValue && t.Value.TotalMinutes == 30),
        It.IsAny<bool>(),
        It.IsAny<When>(),
        It.IsAny<CommandFlags>()), Times.Once);
  }

  [Fact]
  public async Task DeleteSessionAsyncCallsRedisAsync()
  {
    _ = _mockDatabase.Setup(d => d.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
        .ReturnsAsync(true);

    await _store.DeleteSessionAsync(123UL, 456UL);

    _mockDatabase.Verify(d => d.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()), Times.Once);
  }

  [Fact]
  public async Task GetSessionAsyncRedisThrowsReturnsNullAsync()
  {
    _ = _mockDatabase.Setup(d => d.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
        .ThrowsAsync(new RedisConnectionException(ConnectionFailureType.UnableToConnect, "Test error"));

    DailyAlertSetupSession? result = await _store.GetSessionAsync(123UL, 456UL);

    Assert.Null(result);
  }
}
