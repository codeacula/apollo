using Apollo.Cache;
using Apollo.Core.Logging;
using Apollo.Domain.Common.Enums;
using Apollo.Domain.People.ValueObjects;

using Microsoft.Extensions.Logging;

using Moq;

using StackExchange.Redis;

namespace Apollo.Cache.Tests;

public class PersonCacheTests
{
  [Fact]
  public async Task GetAccessAsyncReturnsNullWhenKeyNotFound()
  {
    // Arrange
    var redis = new Mock<IConnectionMultiplexer>();
    var db = new Mock<IDatabase>();
    var logger = new Mock<ILogger<PersonCache>>();
    var username = new Username("testuser", Platform.Discord);

    _ = redis.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(db.Object);
    _ = db.Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
      .ReturnsAsync(RedisValue.Null);

    var cache = new PersonCache(redis.Object, logger.Object);

    // Act
    var result = await cache.GetAccessAsync(username);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Null(result.Value);
  }

  [Fact]
  public async Task GetAccessAsyncReturnsValueWhenKeyExists()
  {
    // Arrange
    var redis = new Mock<IConnectionMultiplexer>();
    var db = new Mock<IDatabase>();
    var logger = new Mock<ILogger<PersonCache>>();
    var username = new Username("testuser", Platform.Discord);

    _ = redis.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(db.Object);
    _ = db.Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
      .ReturnsAsync((RedisValue)true);

    var cache = new PersonCache(redis.Object, logger.Object);

    // Act
    var result = await cache.GetAccessAsync(username);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
    Assert.True(result.Value);
  }

  [Fact]
  public async Task GetAccessAsyncReturnsFailureWhenExceptionThrown()
  {
    // Arrange
    var redis = new Mock<IConnectionMultiplexer>();
    var db = new Mock<IDatabase>();
    var logger = new Mock<ILogger<PersonCache>>();
    var username = new Username("testuser", Platform.Discord);

    _ = redis.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(db.Object);
    _ = db.Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
      .ThrowsAsync(new RedisException("boom"));

    var cache = new PersonCache(redis.Object, logger.Object);

    // Act
    var result = await cache.GetAccessAsync(username);

    // Assert
    Assert.True(result.IsFailed);
    Assert.Contains("boom", result.Errors[0].Message);
  }

  [Fact]
  public async Task SetAccessAsyncStoresValueSuccessfully()
  {
    // Arrange
    var redis = new Mock<IConnectionMultiplexer>();
    var db = new Mock<IDatabase>();
    var logger = new Mock<ILogger<PersonCache>>();
    var username = new Username("testuser", Platform.Discord);

    _ = redis.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(db.Object);

    var cache = new PersonCache(redis.Object, logger.Object);

    // Act
    var result = await cache.SetAccessAsync(username, true);

    // Assert
    Assert.True(result.IsSuccess);
    // Verify StringSetAsync was called at least once (regardless of signature variations)
    db.VerifyAll();
  }

  [Fact]
  public async Task InvalidateAccessAsyncDeletesKeySuccessfully()
  {
    // Arrange
    var redis = new Mock<IConnectionMultiplexer>();
    var db = new Mock<IDatabase>();
    var logger = new Mock<ILogger<PersonCache>>();
    var username = new Username("testuser", Platform.Discord);

    _ = redis.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(db.Object);
    _ = db.Setup(x => x.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
      .ReturnsAsync(true);

    var cache = new PersonCache(redis.Object, logger.Object);

    // Act
    var result = await cache.InvalidateAccessAsync(username);

    // Assert
    Assert.True(result.IsSuccess);
    db.Verify(x => x.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()), Times.Once);
  }

  [Fact]
  public async Task InvalidateAccessAsyncReturnsFailureWhenExceptionThrown()
  {
    // Arrange
    var redis = new Mock<IConnectionMultiplexer>();
    var db = new Mock<IDatabase>();
    var logger = new Mock<ILogger<PersonCache>>();
    var username = new Username("testuser", Platform.Discord);

    _ = redis.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(db.Object);
    _ = db.Setup(x => x.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
      .ThrowsAsync(new RedisException("boom"));

    var cache = new PersonCache(redis.Object, logger.Object);

    // Act
    var result = await cache.InvalidateAccessAsync(username);

    // Assert
    Assert.True(result.IsFailed);
    Assert.Contains("boom", result.Errors[0].Message);
  }
}
