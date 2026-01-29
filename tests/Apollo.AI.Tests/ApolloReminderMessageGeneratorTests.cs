using Apollo.AI.Requests;
using Apollo.Core.People;
using Apollo.Domain.Common.Enums;
using Apollo.Domain.Common.ValueObjects;
using Apollo.Domain.People.Models;
using Apollo.Domain.People.ValueObjects;

using Moq;

namespace Apollo.AI.Tests;

public class ApolloReminderMessageGeneratorTests
{
  private readonly Mock<IApolloAIAgent> _mockAIAgent;
  private readonly Mock<IAIRequestBuilder> _mockRequestBuilder;
  private readonly TimeProvider _mockTimeProvider;
  private readonly PersonConfig _personConfig;
  private readonly ApolloReminderMessageGenerator _generator;
  private readonly DateTimeOffset _fixedUtcTime;

  public ApolloReminderMessageGeneratorTests()
  {
    _mockAIAgent = new Mock<IApolloAIAgent>();
    _mockRequestBuilder = new Mock<IAIRequestBuilder>();
    // Fixed UTC time: 2025-12-12 20:30:00 UTC (8:30 PM UTC)
    _fixedUtcTime = new DateTimeOffset(2025, 12, 12, 20, 30, 0, TimeSpan.Zero);
    _mockTimeProvider = new FixedTimeProvider(_fixedUtcTime);
    _personConfig = new PersonConfig { DefaultTimeZoneId = "America/Chicago" };
    _generator = new ApolloReminderMessageGenerator(_mockAIAgent.Object, _mockTimeProvider, _personConfig);
  }

  [Fact]
  public async Task GenerateReminderMessageConvertsUtcToUserTimeZoneAsync()
  {
    // Arrange
    _ = PersonTimeZoneId.TryParse("America/New_York", out var timeZoneId, out _);
    var person = new Person
    {
      Id = new PersonId(Guid.NewGuid()),
      PlatformId = new PlatformId("testuser", "test-platform-id", Platform.Discord),
      Username = new Username("testuser"),
      HasAccess = new HasAccess(true),
      TimeZoneId = timeZoneId,
      CreatedOn = new CreatedOn(DateTime.UtcNow),
      UpdatedOn = new UpdatedOn(DateTime.UtcNow)
    };

    var toDoDescriptions = new[] { "Buy groceries" };
    // UTC 20:30 -> EST 15:30 (3:30 PM) with offset -05:00
    const string expectedLocalTime = "2025-12-12T15:30:00-05:00";

    string capturedTime = null!;
    string capturedTimezone = null!;

    _ = _mockAIAgent
      .Setup(x => x.CreateReminderRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
      .Callback<string, string, string>((tz, time, _) =>
      {
        capturedTimezone = tz;
        capturedTime = time;
      })
      .Returns(_mockRequestBuilder.Object);

    _ = _mockRequestBuilder
      .Setup(x => x.ExecuteAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(AIRequestResult.SuccessWithContent("It's 3:30pm - Buy groceries!"));

    // Act
    var result = await _generator.GenerateReminderMessageAsync(person, toDoDescriptions);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal("America/New_York", capturedTimezone);
    Assert.Equal(expectedLocalTime, capturedTime);
  }

  [Fact]
  public async Task GenerateReminderMessageUsesDefaultTimeZoneWhenUserHasNoneAsync()
  {
    // Arrange
    var person = new Person
    {
      Id = new PersonId(Guid.NewGuid()),
      PlatformId = new PlatformId("testuser", "test-platform-id", Platform.Discord),
      Username = new Username("testuser"),
      HasAccess = new HasAccess(true),
      TimeZoneId = null, // No timezone set
      CreatedOn = new CreatedOn(DateTime.UtcNow),
      UpdatedOn = new UpdatedOn(DateTime.UtcNow)
    };

    var toDoDescriptions = new[] { "Take medication" };
    // UTC 20:30 -> CST 14:30 (2:30 PM) with offset -06:00
    const string expectedLocalTime = "2025-12-12T14:30:00-06:00";

    string capturedTime = null!;
    string capturedTimezone = null!;

    _ = _mockAIAgent
      .Setup(x => x.CreateReminderRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
      .Callback<string, string, string>((tz, time, _) =>
      {
        capturedTimezone = tz;
        capturedTime = time;
      })
      .Returns(_mockRequestBuilder.Object);

    _ = _mockRequestBuilder
      .Setup(x => x.ExecuteAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(AIRequestResult.SuccessWithContent("It's 2:30pm - Take medication!"));

    // Act
    var result = await _generator.GenerateReminderMessageAsync(person, toDoDescriptions);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal("America/Chicago", capturedTimezone);
    Assert.Equal(expectedLocalTime, capturedTime);
  }

  [Fact]
  public async Task GenerateReminderMessageHandlesPacificTimeZoneAsync()
  {
    // Arrange
    _ = PersonTimeZoneId.TryParse("America/Los_Angeles", out var timeZoneId, out _);
    var person = new Person
    {
      Id = new PersonId(Guid.NewGuid()),
      PlatformId = new PlatformId("testuser", "test-platform-id", Platform.Discord),
      Username = new Username("testuser"),
      HasAccess = new HasAccess(true),
      TimeZoneId = timeZoneId,
      CreatedOn = new CreatedOn(DateTime.UtcNow),
      UpdatedOn = new UpdatedOn(DateTime.UtcNow)
    };

    var toDoDescriptions = new[] { "Start streaming" };
    // UTC 20:30 -> PST 12:30 (12:30 PM) with offset -08:00
    const string expectedLocalTime = "2025-12-12T12:30:00-08:00";

    string capturedTime = null!;

    _ = _mockAIAgent
      .Setup(x => x.CreateReminderRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
      .Callback<string, string, string>((_, time, _) => capturedTime = time)
      .Returns(_mockRequestBuilder.Object);

    _ = _mockRequestBuilder
      .Setup(x => x.ExecuteAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(AIRequestResult.SuccessWithContent("It's 12:30pm - Start streaming!"));

    // Act
    var result = await _generator.GenerateReminderMessageAsync(person, toDoDescriptions);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(expectedLocalTime, capturedTime);
  }

  [Fact]
  public async Task GenerateReminderMessageHandlesInvalidTimeZoneAsync()
  {
    // Arrange - create a person with a manually constructed PersonTimeZoneId that bypasses TryParse validation
    // This simulates a corrupted database state or edge case
    var person = new Person
    {
      Id = new PersonId(Guid.NewGuid()),
      PlatformId = new PlatformId("testuser", "test-platform-id", Platform.Discord),
      Username = new Username("testuser"),
      HasAccess = new HasAccess(true),
      TimeZoneId = null, // Will use default timezone
      CreatedOn = new CreatedOn(DateTime.UtcNow),
      UpdatedOn = new UpdatedOn(DateTime.UtcNow)
    };

    // Create generator with invalid default timezone config
    var invalidConfig = new PersonConfig { DefaultTimeZoneId = "Invalid/Timezone" };
    var generatorWithInvalidConfig = new ApolloReminderMessageGenerator(_mockAIAgent.Object, _mockTimeProvider, invalidConfig);

    var toDoDescriptions = new[] { "Test task" };

    // Act
    var result = await generatorWithInvalidConfig.GenerateReminderMessageAsync(person, toDoDescriptions);

    // Assert
    Assert.True(result.IsFailed);
    Assert.Contains("Timezone", result.Errors[0].Message);
  }

  private sealed class FixedTimeProvider(DateTimeOffset fixedTime) : TimeProvider
  {
    private readonly DateTimeOffset _fixedTime = fixedTime;

    public override DateTimeOffset GetUtcNow() => _fixedTime;
  }
}
