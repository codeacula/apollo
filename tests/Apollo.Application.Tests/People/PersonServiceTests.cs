using Apollo.Application.People;
using Apollo.Core.People;
using Apollo.Domain.Common.Enums;
using Apollo.Domain.Common.ValueObjects;
using Apollo.Domain.People.Models;
using Apollo.Domain.People.ValueObjects;

using FluentResults;

using Microsoft.Extensions.Logging;

using Moq;

namespace Apollo.Application.Tests.People;

public class PersonServiceTests
{
  private readonly Mock<IPersonStore> _mockPersonStore;
  private readonly Mock<IPersonCache> _mockPersonCache;
  private readonly Mock<ILogger<PersonService>> _mockLogger;
  private readonly PersonService _personService;

  public PersonServiceTests()
  {
    _mockPersonStore = new Mock<IPersonStore>();
    _mockPersonCache = new Mock<IPersonCache>();
    _mockLogger = new Mock<ILogger<PersonService>>();
    _personService = new PersonService(_mockPersonStore.Object, _mockPersonCache.Object, _mockLogger.Object);
  }

  private static PlatformId GetPlatformId()
  {
    return new PlatformId("testuser", "123", Platform.Discord);
  }

  [Fact]
  public async Task GetOrCreateAsyncWithExistingUserReturnsUserAsync()
  {
    // Arrange
    var platformId = new PlatformId("testuser", "123", Platform.Discord);
    var personId = new PersonId(Guid.NewGuid());
    var expectedPerson = new Person
    {
      Id = personId,
      Username = new Username("testuser"),
      HasAccess = new HasAccess(true),
      CreatedOn = new CreatedOn(DateTime.UtcNow),
      UpdatedOn = new UpdatedOn(DateTime.UtcNow)
    };

    _ = _mockPersonStore
      .Setup(x => x.GetByPlatformIdAsync(platformId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(expectedPerson));

    // Act
    var result = await _personService.GetOrCreateAsync(platformId);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(expectedPerson.Id, result.Value.Id);
    Assert.Equal(expectedPerson.Username, result.Value.Username);
    _mockPersonStore.Verify(x => x.CreateByPlatformIdAsync(It.IsAny<PlatformId>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task GetOrCreateAsyncCreatesNewUserWhenNotFoundAsync()
  {
    // Arrange
    var platformId = new PlatformId("newuser", "456", Platform.Discord);
    var personId = new PersonId(Guid.NewGuid());
    var createdPerson = new Person
    {
      Id = personId,
      Username = new Username("newuser"),
      HasAccess = new HasAccess(false),
      CreatedOn = new CreatedOn(DateTime.UtcNow),
      UpdatedOn = new UpdatedOn(DateTime.UtcNow)
    };

    _ = _mockPersonStore
      .Setup(x => x.GetByPlatformIdAsync(platformId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Fail<Person>("User not found"));

    _ = _mockPersonStore
      .Setup(x => x.CreateByPlatformIdAsync(platformId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(createdPerson));

    // Act
    var result = await _personService.GetOrCreateAsync(platformId);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(createdPerson.Username, result.Value.Username);
    _mockPersonStore.Verify(x => x.CreateByPlatformIdAsync(platformId, It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task GetOrCreateAsyncReturnsFailureWhenBothGetAndCreateFailAsync()
  {
    // Arrange
    var platformId = new PlatformId("failuser", "789", Platform.Discord);

    _ = _mockPersonStore
      .Setup(x => x.GetByPlatformIdAsync(platformId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Fail<Person>("User not found"));

    _ = _mockPersonStore
      .Setup(x => x.CreateByPlatformIdAsync(platformId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Fail<Person>("Database error"));

    // Act
    var result = await _personService.GetOrCreateAsync(platformId);

    // Assert
    Assert.True(result.IsFailed);
    Assert.Contains("Failed to get or create user", result.Errors[0].Message);
  }

  [Fact]
  public async Task HasAccessAsyncWithValidPersonIdReturnsCachedValueAsync()
  {
    // Arrange
    var platformId = GetPlatformId();

    _ = _mockPersonCache
      .Setup(x => x.GetAccessAsync(platformId))
      .ReturnsAsync(Result.Ok<bool?>(true));

    // Act
    var result = await _personService.HasAccessAsync(platformId);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.True(result.Value.Value);
  }

  [Fact]
  public async Task HasAccessAsyncWhenCacheFailsReturnsFailureAsync()
  {
    // Arrange
    var platformId = GetPlatformId();

    _ = _mockPersonCache
      .Setup(x => x.GetAccessAsync(platformId))
      .ReturnsAsync(Result.Fail<bool?>("Cache error"));

    // Act
    var result = await _personService.HasAccessAsync(platformId);

    // Assert
    Assert.True(result.IsFailed);
    Assert.Contains("fail-closed policy denies access", result.Errors[0].Message);
  }

  [Fact]
  public async Task HasAccessAsyncWhenCacheReturnsNullReturnsTrueAsync()
  {
    // Arrange
    var platformId = GetPlatformId();

    _ = _mockPersonCache
      .Setup(x => x.GetAccessAsync(platformId))
      .ReturnsAsync(Result.Ok<bool?>(null));

    // Act
    var result = await _personService.HasAccessAsync(platformId);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.True(result.Value.Value);
  }

  [Fact]
  public async Task HasAccessAsyncWhenCacheReturnsFalseReturnsFalseAsync()
  {
    // Arrange
    var platformId = GetPlatformId();

    _ = _mockPersonCache
      .Setup(x => x.GetAccessAsync(platformId))
      .ReturnsAsync(Result.Ok<bool?>(false));

    // Act
    var result = await _personService.HasAccessAsync(platformId);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.False(result.Value.Value);
  }

  [Fact]
  public async Task MapPlatformIdToPersonIdAsyncSucceedsWhenCacheSucceedsAsync()
  {
    // Arrange
    var platformId = new PlatformId("testuser", "123", Platform.Discord);
    var personId = new PersonId(Guid.NewGuid());

    _ = _mockPersonCache
      .Setup(x => x.MapPlatformIdToPersonIdAsync(platformId, personId))
      .ReturnsAsync(Result.Ok());

    // Act
    var result = await _personService.MapPlatformIdToPersonIdAsync(platformId, personId);

    // Assert
    Assert.True(result.IsSuccess);
    _mockPersonCache.Verify(x => x.MapPlatformIdToPersonIdAsync(platformId, personId), Times.Once);
  }

  [Fact]
  public async Task MapPlatformIdToPersonIdAsyncReturnsFailureWhenCacheFailsAsync()
  {
    // Arrange
    var platformId = new PlatformId("testuser", "123", Platform.Discord);
    var personId = new PersonId(Guid.NewGuid());

    _ = _mockPersonCache
      .Setup(x => x.MapPlatformIdToPersonIdAsync(platformId, personId))
      .ReturnsAsync(Result.Fail("Cache write error"));

    // Act
    var result = await _personService.MapPlatformIdToPersonIdAsync(platformId, personId);

    // Assert
    Assert.True(result.IsFailed);
    Assert.Contains("Failed to map platform ID to person ID", result.Errors[0].Message);
  }
}
