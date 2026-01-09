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

  [Fact]
  public async Task GetOrCreateAsyncWithValidUsernameReturnsExistingUserAsync()
  {
    // Arrange
    var username = new Username("testuser");
    var personId = new PersonId(Platform.Discord, "123");
    var expectedPerson = new Person
    {
      Id = personId,
      Username = username,
      HasAccess = new HasAccess(true),
      CreatedOn = new CreatedOn(DateTime.UtcNow),
      UpdatedOn = new UpdatedOn(DateTime.UtcNow)
    };

    _ = _mockPersonStore
      .Setup(x => x.GetAsync(personId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(expectedPerson));

    // Act
    var result = await _personService.GetOrCreateAsync(personId, username);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(expectedPerson.Id, result.Value.Id);
    Assert.Equal(expectedPerson.Username, result.Value.Username);
    _mockPersonStore.Verify(x => x.CreateAsync(It.IsAny<PersonId>(), It.IsAny<Username>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task GetOrCreateAsyncWithValidUsernameCreatesNewUserWhenNotFoundAsync()
  {
    // Arrange
    var username = new Username("newuser");
    var personId = new PersonId(Platform.Discord, "123");
    var createdPerson = new Person
    {
      Id = personId,
      Username = username,
      HasAccess = new HasAccess(false),
      CreatedOn = new CreatedOn(DateTime.UtcNow),
      UpdatedOn = new UpdatedOn(DateTime.UtcNow)
    };

    _ = _mockPersonStore
      .Setup(x => x.GetAsync(personId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Fail<Person>("User not found"));

    _ = _mockPersonStore
      .Setup(x => x.CreateAsync(personId, username, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(createdPerson));

    // Act
    var result = await _personService.GetOrCreateAsync(personId, username);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(createdPerson.Username, result.Value.Username);
    _mockPersonStore.Verify(x => x.CreateAsync(personId, username, It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task GetOrCreateAsyncWithInvalidUsernameReturnsFailureAsync()
  {
    // Arrange
    var username = new Username(string.Empty);
    var personId = new PersonId(Platform.Discord, "123");

    // Act
    var result = await _personService.GetOrCreateAsync(personId, username);

    // Assert
    Assert.True(result.IsFailed);
    Assert.Contains("Invalid username", result.Errors[0].Message);
  }

  [Fact]
  public async Task GrantAccessAsyncWithValidUsernameGrantsAccessAsync()
  {
    // Arrange
    var username = new Username("testuser");
    var personId = new PersonId(Platform.Discord, "123");
    var person = new Person
    {
      Id = personId,
      Username = username,
      HasAccess = new HasAccess(false),
      CreatedOn = new CreatedOn(DateTime.UtcNow),
      UpdatedOn = new UpdatedOn(DateTime.UtcNow)
    };

    _ = _mockPersonStore
      .Setup(x => x.GetAsync(personId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(person));

    _ = _mockPersonStore
      .Setup(x => x.GrantAccessAsync(person.Id, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok());

    // Act
    var result = await _personService.GrantAccessAsync(personId);

    // Assert
    Assert.True(result.IsSuccess);
    _mockPersonStore.Verify(x => x.GrantAccessAsync(person.Id, It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task GrantAccessAsyncWithInvalidPersonIdReturnsFailureAsync()
  {
    // Arrange
    var personId = new PersonId(Platform.Discord, string.Empty);

    // Act
    var result = await _personService.GrantAccessAsync(personId);

    // Assert
    Assert.True(result.IsFailed);
    Assert.Contains("Invalid username", result.Errors[0].Message);
  }

  [Fact]
  public async Task GrantAccessAsyncWithNonExistentUserReturnsFailureAsync()
  {
    // Arrange
    var personId = new PersonId(Platform.Discord, "123");

    _ = _mockPersonStore
      .Setup(x => x.GetAsync(personId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Fail<Person>("User not found"));

    // Act
    var result = await _personService.GrantAccessAsync(personId);

    // Assert
    Assert.True(result.IsFailed);
    Assert.Contains("not found", result.Errors[0].Message);
  }

  [Fact]
  public async Task HasAccessAsyncWithValidPersonIdReturnsCachedValueAsync()
  {
    // Arrange
    var personId = new PersonId(Platform.Discord, "123");

    _ = _mockPersonCache
      .Setup(x => x.GetAccessAsync(personId))
      .ReturnsAsync(Result.Ok<bool?>(true));

    // Act
    var result = await _personService.HasAccessAsync(personId);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.True(result.Value.Value);
  }

  [Fact]
  public async Task HasAccessAsyncWithInvalidPersonIdReturnsFailureAsync()
  {
    // Arrange
    var personId = new PersonId(Platform.Discord, "");

    // Act
    var result = await _personService.HasAccessAsync(personId);

    // Assert
    Assert.True(result.IsFailed);
    Assert.Contains("Invalid username", result.Errors[0].Message);
  }

  [Fact]
  public async Task HasAccessAsyncWhenCacheFailsReturnsFailureAsync()
  {
    // Arrange
    var personId = new PersonId(Platform.Discord, "123");

    _ = _mockPersonCache
      .Setup(x => x.GetAccessAsync(personId))
      .ReturnsAsync(Result.Fail<bool?>("Cache error"));

    // Act
    var result = await _personService.HasAccessAsync(personId);

    // Assert
    Assert.True(result.IsFailed);
    Assert.Contains("fail-closed policy denies access", result.Errors[0].Message);
  }

  [Fact]
  public async Task HasAccessAsyncWhenCacheReturnsNullReturnsTrueAsync()
  {
    // Arrange
    var personId = new PersonId(Platform.Discord, "123");

    _ = _mockPersonCache
      .Setup(x => x.GetAccessAsync(personId))
      .ReturnsAsync(Result.Ok<bool?>(null));

    // Act
    var result = await _personService.HasAccessAsync(personId);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.True(result.Value.Value);
  }
}
