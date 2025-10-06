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

        _mockRedis.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(_mockDatabase.Object);

        _store = new RedisDailyAlertSetupSessionStore(_mockRedis.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetSessionAsync_NoSession_ReturnsNull()
    {
        _mockDatabase.Setup(d => d.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);

        var result = await _store.GetSessionAsync(123UL, 456UL);

        Assert.Null(result);
    }

    [Fact]
    public async Task SetSessionAsync_CallsRedisWithCorrectTtl()
    {
        var session = new DailyAlertSetupSession
        {
            ChannelId = 123UL,
            RoleId = 456UL,
            Time = "08:00",
            Message = "Test"
        };

        _mockDatabase.Setup(d => d.StringSetAsync(
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
    public async Task DeleteSessionAsync_CallsRedis()
    {
        _mockDatabase.Setup(d => d.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        await _store.DeleteSessionAsync(123UL, 456UL);

        _mockDatabase.Verify(d => d.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()), Times.Once);
    }

    [Fact]
    public async Task GetSessionAsync_RedisThrows_ReturnsNull()
    {
        _mockDatabase.Setup(d => d.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ThrowsAsync(new RedisConnectionException(ConnectionFailureType.UnableToConnect, "Test error"));

        var result = await _store.GetSessionAsync(123UL, 456UL);

        Assert.Null(result);
    }

    [Fact]
    public async Task SetSessionAsync_RedisThrows_ThrowsException()
    {
        _mockDatabase.Setup(d => d.StringSetAsync(
            It.IsAny<RedisKey>(),
            It.IsAny<RedisValue>(),
            It.IsAny<TimeSpan?>(),
            It.IsAny<bool>(),
            It.IsAny<When>(),
            It.IsAny<CommandFlags>()))
            .ThrowsAsync(new RedisConnectionException(ConnectionFailureType.UnableToConnect, "Test error"));

        var session = new DailyAlertSetupSession();

        await Assert.ThrowsAsync<RedisConnectionException>(
            async () => await _store.SetSessionAsync(123UL, 456UL, session));
    }
}
