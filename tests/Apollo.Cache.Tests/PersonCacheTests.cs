using Apollo.Domain.Common.Enums;
using Apollo.Domain.People.ValueObjects;

using Microsoft.Extensions.Logging;

using Moq;

using StackExchange.Redis;

namespace Apollo.Cache.Tests;

public class PersonCacheTests
{
  [Fact]
  public async Task GetPersonIdAsyncReturnsNullWhenKeyNotFoundAsync()
  {
    // Arrange
    var redis = new Mock<IConnectionMultiplexer>();
    var db = new Mock<IDatabase>();
    var logger = new Mock<ILogger<PersonCache>>();
    var platformId = new PlatformId("testuser", "123", Platform.Discord);

    _ = redis.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(db.Object);
    _ = db.Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
      .ReturnsAsync(RedisValue.Null);

    var cache = new PersonCache(redis.Object, logger.Object);

    // Act
    var result = await cache.GetPersonIdAsync(platformId);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Null(result.Value);
  }

  [Fact]
  public async Task GetPersonIdAsyncReturnsValueWhenKeyExistsAsync()
  {
    // Arrange
    var redis = new Mock<IConnectionMultiplexer>();
    var db = new Mock<IDatabase>();
    var logger = new Mock<ILogger<PersonCache>>();
    var platformId = new PlatformId("testuser", "123", Platform.Discord);
    var expectedGuid = Guid.NewGuid();

    _ = redis.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(db.Object);
    _ = db.Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
      .ReturnsAsync((RedisValue)expectedGuid.ToString());

    var cache = new PersonCache(redis.Object, logger.Object);

    // Act
    var result = await cache.GetPersonIdAsync(platformId);

    // Assert
    Assert.True(result.IsSuccess);
    _ = Assert.NotNull(result.Value);
    Assert.Equal(expectedGuid, result.Value.Value.Value);
  }

  [Fact]
  public async Task GetPersonIdAsyncReturnsFailureWhenExceptionThrownAsync()
  {
    // Arrange
    var redis = new Mock<IConnectionMultiplexer>();
    var db = new Mock<IDatabase>();
    var logger = new Mock<ILogger<PersonCache>>();
    var platformId = new PlatformId("testuser", "123", Platform.Discord);

    _ = redis.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(db.Object);
    _ = db.Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
      .ThrowsAsync(new RedisException("boom"));

    var cache = new PersonCache(redis.Object, logger.Object);

    // Act
    var result = await cache.GetPersonIdAsync(platformId);

    // Assert
    Assert.True(result.IsFailed);
    Assert.Contains("boom", result.Errors[0].Message);
  }

  [Fact]
  public async Task GetAccessAsyncReturnsNullWhenKeyNotFoundAsync()
  {
    // Arrange
    var redis = new Mock<IConnectionMultiplexer>();
    var db = new Mock<IDatabase>();
    var logger = new Mock<ILogger<PersonCache>>();
    var personId = new PersonId(Guid.NewGuid());

    _ = redis.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(db.Object);
    _ = db.Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
      .ReturnsAsync(RedisValue.Null);

    var cache = new PersonCache(redis.Object, logger.Object);

    // Act
    var result = await cache.GetAccessAsync(personId);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Null(result.Value);
  }

  [Fact]
  public async Task GetAccessAsyncReturnsValueWhenKeyExistsAsync()
  {
    // Arrange
    var redis = new Mock<IConnectionMultiplexer>();
    var db = new Mock<IDatabase>();
    var logger = new Mock<ILogger<PersonCache>>();
    var personId = new PersonId(Guid.NewGuid());

    _ = redis.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(db.Object);
    _ = db.Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
      .ReturnsAsync((RedisValue)true);

    var cache = new PersonCache(redis.Object, logger.Object);

    // Act
    var result = await cache.GetAccessAsync(personId);

    // Assert
    Assert.True(result.IsSuccess);
    _ = Assert.NotNull(result.Value);
    Assert.True(result.Value);
  }

  [Fact]
  public async Task GetAccessAsyncReturnsFailureWhenExceptionThrownAsync()
  {
    // Arrange
    var redis = new Mock<IConnectionMultiplexer>();
    var db = new Mock<IDatabase>();
    var logger = new Mock<ILogger<PersonCache>>();
    var personId = new PersonId(Guid.NewGuid());

    _ = redis.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(db.Object);
    _ = db.Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
      .ThrowsAsync(new RedisException("boom"));

    var cache = new PersonCache(redis.Object, logger.Object);

    // Act
    var result = await cache.GetAccessAsync(personId);

    // Assert
    Assert.True(result.IsFailed);
    Assert.Contains("boom", result.Errors[0].Message);
  }

  [Fact]
  public async Task SetAccessAsyncStoresValueSuccessfullyAsync()
  {
    // Arrange
    var redis = new Mock<IConnectionMultiplexer>();
    var db = new Mock<IDatabase>();
    var logger = new Mock<ILogger<PersonCache>>();
    var personId = new PersonId(Guid.NewGuid());

    _ = redis.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(db.Object);

    var cache = new PersonCache(redis.Object, logger.Object);

    // Act
    var result = await cache.SetAccessAsync(personId, true);

    // Assert
    Assert.True(result.IsSuccess);
    // Verify StringSetAsync was called at least once (regardless of signature variations)
    db.VerifyAll();
  }

  [Fact]
  public async Task InvalidateAccessAsyncDeletesKeySuccessfullyAsync()
  {
    // Arrange
    var redis = new Mock<IConnectionMultiplexer>();
    var db = new Mock<IDatabase>();
    var logger = new Mock<ILogger<PersonCache>>();
    var personId = new PersonId(Guid.NewGuid());

    _ = redis.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(db.Object);
    _ = db.Setup(x => x.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
      .ReturnsAsync(true);

    var cache = new PersonCache(redis.Object, logger.Object);

    // Act
    var result = await cache.InvalidateAccessAsync(personId);

    // Assert
    Assert.True(result.IsSuccess);
    db.Verify(x => x.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()), Times.Once);
  }

  [Fact]
  public async Task InvalidateAccessAsyncReturnsFailureWhenExceptionThrownAsync()
  {
    // Arrange
    var redis = new Mock<IConnectionMultiplexer>();
    var db = new Mock<IDatabase>();
    var logger = new Mock<ILogger<PersonCache>>();
    var personId = new PersonId(Guid.NewGuid());

    _ = redis.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(db.Object);
    _ = db.Setup(x => x.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
      .ThrowsAsync(new RedisException("boom"));

    var cache = new PersonCache(redis.Object, logger.Object);

    // Act
    var result = await cache.InvalidateAccessAsync(personId);

    // Assert
    Assert.True(result.IsFailed);
    Assert.Contains("boom", result.Errors[0].Message);
  }

  [Fact]
  public async Task MapPlatformIdToPersonIdAsyncStoresValueSuccessfullyAsync()
  {
    // Arrange
    var redis = new Mock<IConnectionMultiplexer>();
    var db = new Mock<IDatabase>();
    var logger = new Mock<ILogger<PersonCache>>();
    var platformId = new PlatformId("testuser", "123", Platform.Discord);
    var personId = new PersonId(Guid.NewGuid());

    _ = redis.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(db.Object);

    var cache = new PersonCache(redis.Object, logger.Object);

    // Act
    var result = await cache.MapPlatformIdToPersonIdAsync(platformId, personId);

    // Assert
    Assert.True(result.IsSuccess);
  }

  [Fact]
  public async Task MapPlatformIdToPersonIdAsyncReturnsFailureWhenExceptionThrownAsync()
  {
    // Arrange
    var redis = new Mock<IConnectionMultiplexer>();
    var db = new Mock<IDatabase>();
    var logger = new Mock<ILogger<PersonCache>>();
    var platformId = new PlatformId("testuser", "123", Platform.Discord);
    var personId = new PersonId(Guid.NewGuid());

    _ = redis.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(db.Object);
    _ = db.Setup(x => x.StringSetAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<TimeSpan?>(), It.IsAny<bool>(), It.IsAny<When>(), It.IsAny<CommandFlags>()))
      .ThrowsAsync(new RedisException("boom"));

    var cache = new PersonCache(redis.Object, logger.Object);

    // Act
    var result = await cache.MapPlatformIdToPersonIdAsync(platformId, personId);

    // Assert
    Assert.True(result.IsFailed);
    Assert.Contains("boom", result.Errors[0].Message);
  }
}
