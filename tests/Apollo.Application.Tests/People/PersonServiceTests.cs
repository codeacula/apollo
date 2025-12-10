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
  public async Task GetOrCreateAsyncWithValidUsernameReturnsExistingUser()
  {
    // Arrange
    var username = new Username("testuser", Platform.Discord);
    var expectedPerson = new Person
    {
      Id = new PersonId(Guid.NewGuid()),
      Username = username,
      HasAccess = new HasAccess(true),
      CreatedOn = new CreatedOn(DateTime.UtcNow),
      UpdatedOn = new UpdatedOn(DateTime.UtcNow)
    };

    _ = _mockPersonStore
      .Setup(x => x.GetByUsernameAsync(username, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(expectedPerson));

    // Act
    var result = await _personService.GetOrCreateAsync(username);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(expectedPerson.Id, result.Value.Id);
    Assert.Equal(expectedPerson.Username, result.Value.Username);
    _mockPersonStore.Verify(x => x.CreateAsync(It.IsAny<PersonId>(), It.IsAny<Username>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task GetOrCreateAsyncWithValidUsernameCreatesNewUserWhenNotFound()
  {
    // Arrange
    var username = new Username("newuser", Platform.Discord);
    var createdPerson = new Person
    {
      Id = new PersonId(Guid.NewGuid()),
      Username = username,
      HasAccess = new HasAccess(false),
      CreatedOn = new CreatedOn(DateTime.UtcNow),
      UpdatedOn = new UpdatedOn(DateTime.UtcNow)
    };

    _ = _mockPersonStore
      .Setup(x => x.GetByUsernameAsync(username, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Fail<Person>("User not found"));

    _ = _mockPersonStore
      .Setup(x => x.CreateAsync(It.IsAny<PersonId>(), username, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(createdPerson));

    // Act
    var result = await _personService.GetOrCreateAsync(username);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(createdPerson.Username, result.Value.Username);
    _mockPersonStore.Verify(x => x.CreateAsync(It.IsAny<PersonId>(), username, It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task GetOrCreateAsyncWithInvalidUsernameReturnsFailure()
  {
    // Arrange
    var username = new Username("", Platform.Discord);

    // Act
    var result = await _personService.GetOrCreateAsync(username);

    // Assert
    Assert.True(result.IsFailed);
    Assert.Contains("Invalid username", result.Errors[0].Message);
  }

  [Fact]
  public async Task GrantAccessAsyncWithValidUsernameGrantsAccess()
  {
    // Arrange
    var username = new Username("testuser", Platform.Discord);
    var person = new Person
    {
      Id = new PersonId(Guid.NewGuid()),
      Username = username,
      HasAccess = new HasAccess(false),
      CreatedOn = new CreatedOn(DateTime.UtcNow),
      UpdatedOn = new UpdatedOn(DateTime.UtcNow)
    };

    _ = _mockPersonStore
      .Setup(x => x.GetByUsernameAsync(username, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(person));

    _ = _mockPersonStore
      .Setup(x => x.GrantAccessAsync(person.Id, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok());

    // Act
    var result = await _personService.GrantAccessAsync(username);

    // Assert
    Assert.True(result.IsSuccess);
    _mockPersonStore.Verify(x => x.GrantAccessAsync(person.Id, It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task GrantAccessAsyncWithInvalidUsernameReturnsFailure()
  {
    // Arrange
    var username = new Username("", Platform.Discord);

    // Act
    var result = await _personService.GrantAccessAsync(username);

    // Assert
    Assert.True(result.IsFailed);
    Assert.Contains("Invalid username", result.Errors[0].Message);
  }

  [Fact]
  public async Task GrantAccessAsyncWithNonExistentUserReturnsFailure()
  {
    // Arrange
    var username = new Username("nonexistent", Platform.Discord);

    _ = _mockPersonStore
      .Setup(x => x.GetByUsernameAsync(username, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Fail<Person>("User not found"));

    // Act
    var result = await _personService.GrantAccessAsync(username);

    // Assert
    Assert.True(result.IsFailed);
    Assert.Contains("not found", result.Errors[0].Message);
  }

  [Fact]
  public async Task HasAccessAsyncWithValidUsernameReturnsCachedValue()
  {
    // Arrange
    var username = new Username("testuser", Platform.Discord);

    _ = _mockPersonCache
      .Setup(x => x.GetAccessAsync(username))
      .ReturnsAsync(Result.Ok<bool?>(true));

    // Act
    var result = await _personService.HasAccessAsync(username);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.True(result.Value.Value);
  }

  [Fact]
  public async Task HasAccessAsyncWithInvalidUsernameReturnsFailure()
  {
    // Arrange
    var username = new Username("", Platform.Discord);

    // Act
    var result = await _personService.HasAccessAsync(username);

    // Assert
    Assert.True(result.IsFailed);
    Assert.Contains("Invalid username", result.Errors[0].Message);
  }

  [Fact]
  public async Task HasAccessAsyncWhenCacheFailsReturnsFailure()
  {
    // Arrange
    var username = new Username("testuser", Platform.Discord);

    _ = _mockPersonCache
      .Setup(x => x.GetAccessAsync(username))
      .ReturnsAsync(Result.Fail<bool?>("Cache error"));

    // Act
    var result = await _personService.HasAccessAsync(username);

    // Assert
    Assert.True(result.IsFailed);
    Assert.Contains("fail-closed policy denies access", result.Errors[0].Message);
  }

  [Fact]
  public async Task HasAccessAsyncWhenCacheReturnsNullReturnsTrue()
  {
    // Arrange
    var username = new Username("testuser", Platform.Discord);

    _ = _mockPersonCache
      .Setup(x => x.GetAccessAsync(username))
      .ReturnsAsync(Result.Ok<bool?>(null));

    // Act
    var result = await _personService.HasAccessAsync(username);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.True(result.Value.Value);
  }
}
