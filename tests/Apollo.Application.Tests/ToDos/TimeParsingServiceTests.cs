using Apollo.AI;
using Apollo.AI.Requests;
using Apollo.Application.ToDos;
using Apollo.Core.ToDos;

using FluentResults;

using Moq;

namespace Apollo.Application.Tests.ToDos;

public sealed class TimeParsingServiceTests
{
  private readonly Mock<IFuzzyTimeParser> _fuzzyTimeParser = new();
  private readonly Mock<IApolloAIAgent> _aiAgent = new();
  private readonly Mock<TimeProvider> _timeProvider = new();
  private readonly TimeParsingService _service;

  private static readonly DateTime ReferenceTime = new(2025, 12, 30, 14, 30, 0, DateTimeKind.Utc);

  public TimeParsingServiceTests()
  {
    _ = _timeProvider.Setup(tp => tp.GetUtcNow()).Returns(new DateTimeOffset(ReferenceTime));
    _service = new TimeParsingService(_fuzzyTimeParser.Object, _aiAgent.Object, _timeProvider.Object);
  }

  [Fact]
  public async Task ParseTimeAsyncWithFuzzyTimeReturnsParsedUtcDateTimeAsync()
  {
    // Arrange
    var expected = ReferenceTime.AddMinutes(10);
    _ = _fuzzyTimeParser
      .Setup(p => p.TryParseFuzzyTime("in 10 minutes", ReferenceTime))
      .Returns(Result.Ok(expected));

    // Act
    var result = await _service.ParseTimeAsync("in 10 minutes");

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(expected, result.Value);
    _fuzzyTimeParser.Verify(p => p.TryParseFuzzyTime("in 10 minutes", ReferenceTime), Times.Once);
  }

  [Theory]
  [InlineData("2025-12-31T10:00:00")]
  [InlineData("2025-12-31 10:00:00")]
  [InlineData("2025-12-31 10:00")]
  public async Task ParseTimeAsyncWithIso8601ReturnsUtcDateTimeAsync(string input)
  {
    // Arrange - fuzzy parser fails
    _ = _fuzzyTimeParser
      .Setup(p => p.TryParseFuzzyTime(input, ReferenceTime))
      .Returns(Result.Fail<DateTime>("Not fuzzy"));

    // Act
    var result = await _service.ParseTimeAsync(input);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(new DateTime(2025, 12, 31, 10, 0, 0, DateTimeKind.Utc), result.Value);
  }

  [Theory]
  [InlineData("3:00 PM", 15, 0)]
  [InlineData("9:30 AM", 9, 30)]
  public async Task ParseTimeAsyncWithCommonTimeFormatReturnsUtcDateTimeAsync(
    string input, int expectedHour, int expectedMinute)
  {
    // Arrange - fuzzy parser fails
    _ = _fuzzyTimeParser
      .Setup(p => p.TryParseFuzzyTime(input, ReferenceTime))
      .Returns(Result.Fail<DateTime>("Not fuzzy"));

    // Act
    var result = await _service.ParseTimeAsync(input);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(expectedHour, result.Value.Hour);
    Assert.Equal(expectedMinute, result.Value.Minute);
  }

  [Theory]
  [InlineData("December 31, 2025")]
  [InlineData("Dec 31, 2025")]
  public async Task ParseTimeAsyncWithDateTimeReturnsParsedDateTimeAsync(string input)
  {
    // Arrange - fuzzy parser fails
    _ = _fuzzyTimeParser
      .Setup(p => p.TryParseFuzzyTime(input, ReferenceTime))
      .Returns(Result.Fail<DateTime>("Not fuzzy"));

    // Act
    var result = await _service.ParseTimeAsync(input);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(2025, result.Value.Year);
    Assert.Equal(12, result.Value.Month);
    Assert.Equal(31, result.Value.Day);
  }

  [Fact]
  public async Task ParseTimeAsyncWithInvalidInputCallsLlmFallbackAsync()
  {
    // Arrange - fuzzy parser fails, C# parsing fails, LLM returns UNPARSEABLE
    const string input = "not a time at all";
    _ = _fuzzyTimeParser
      .Setup(p => p.TryParseFuzzyTime(input, ReferenceTime))
      .Returns(Result.Fail<DateTime>("Not fuzzy"));

    var mockBuilder = new Mock<IAIRequestBuilder>();
    _ = _aiAgent.Setup(a => a.CreateTimeParsingRequestAsync(input, "UTC", It.IsAny<string>()))
      .ReturnsAsync(mockBuilder.Object);
    _ = mockBuilder.Setup(b => b.ExecuteAsync(default))
      .ReturnsAsync(AIRequestResult.SuccessWithContent("UNPARSEABLE"));

    // Act
    var result = await _service.ParseTimeAsync(input);

    // Assert
    Assert.True(result.IsFailed);
    Assert.Contains("Invalid time format", result.Errors[0].Message);
  }

  [Fact]
  public async Task ParseTimeAsyncWithUserTimeZoneConvertsToUtcAsync()
  {
    // Arrange - fuzzy parser fails, falls through to C# parsing
    const string input = "2025-12-31T10:00:00";
    _ = _fuzzyTimeParser
      .Setup(p => p.TryParseFuzzyTime(input, ReferenceTime))
      .Returns(Result.Fail<DateTime>("Not fuzzy"));

    // Act - America/Chicago is UTC-6
    var result = await _service.ParseTimeAsync(input, "America/Chicago");

    // Assert
    Assert.True(result.IsSuccess);
    // 10:00 CST = 16:00 UTC
    Assert.Equal(new DateTime(2025, 12, 31, 16, 0, 0, DateTimeKind.Utc), result.Value);
  }

  [Fact]
  public async Task ParseTimeAsyncWithoutUserTimeZoneAssumesUtcAsync()
  {
    // Arrange - fuzzy parser fails
    const string input = "2025-12-31T10:00:00";
    _ = _fuzzyTimeParser
      .Setup(p => p.TryParseFuzzyTime(input, ReferenceTime))
      .Returns(Result.Fail<DateTime>("Not fuzzy"));

    // Act - no timezone specified
    var result = await _service.ParseTimeAsync(input);

    // Assert
    Assert.True(result.IsSuccess);
    // Should treat as UTC since no timezone given
    Assert.Equal(new DateTime(2025, 12, 31, 10, 0, 0, DateTimeKind.Utc), result.Value);
  }

  [Fact]
  public async Task ParseTimeAsyncWithUnspecifiedKindTreatsAsUserLocalAsync()
  {
    // Arrange - fuzzy parser fails, a date without explicit kind
    const string input = "Dec 31, 2025";
    _ = _fuzzyTimeParser
      .Setup(p => p.TryParseFuzzyTime(input, ReferenceTime))
      .Returns(Result.Fail<DateTime>("Not fuzzy"));

    // Act - with timezone
    var result = await _service.ParseTimeAsync(input, "America/New_York");

    // Assert
    Assert.True(result.IsSuccess);
    // Dec 31 midnight EST = Dec 31 05:00 UTC
    Assert.Equal(new DateTime(2025, 12, 31, 5, 0, 0, DateTimeKind.Utc), result.Value);
  }

  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  [InlineData(null)]
  public async Task ParseTimeAsyncWithEmptyInputReturnsFailureAsync(string? input)
  {
    // Act
    var result = await _service.ParseTimeAsync(input!);

    // Assert
    Assert.True(result.IsFailed);
    _fuzzyTimeParser.Verify(p => p.TryParseFuzzyTime(It.IsAny<string>(), It.IsAny<DateTime>()), Times.Never);
  }

  [Fact]
  public async Task ParseTimeAsyncDoesNotCallLlmWhenFuzzyParserSucceedsAsync()
  {
    // Arrange
    var expected = ReferenceTime.AddHours(1);
    _ = _fuzzyTimeParser
      .Setup(p => p.TryParseFuzzyTime("in an hour", ReferenceTime))
      .Returns(Result.Ok(expected));

    // Act
    var result = await _service.ParseTimeAsync("in an hour");

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(expected, result.Value);
    _fuzzyTimeParser.Verify(p => p.TryParseFuzzyTime("in an hour", ReferenceTime), Times.Once);
    _aiAgent.Verify(a => a.CreateTimeParsingRequestAsync(It.IsAny<string>(), It.IsAny<string>(),
      It.IsAny<string>()), Times.Never);
  }

  [Fact]
  public async Task ParseTimeAsyncDoesNotCallLlmWhenCSharpParseSucceedsAsync()
  {
    // Arrange - fuzzy parser fails, but C# parsing succeeds
    const string input = "2025-12-31T10:00:00";
    _ = _fuzzyTimeParser
      .Setup(p => p.TryParseFuzzyTime(input, ReferenceTime))
      .Returns(Result.Fail<DateTime>("Not fuzzy"));

    // Act
    var result = await _service.ParseTimeAsync(input);

    // Assert
    Assert.True(result.IsSuccess);
    _aiAgent.Verify(a => a.CreateTimeParsingRequestAsync(It.IsAny<string>(), It.IsAny<string>(),
      It.IsAny<string>()), Times.Never);
  }

  [Fact]
  public async Task ParseTimeAsyncWhenAllParsersFallsBackToLlmReturnsUtcDateTimeAsync()
  {
    // Arrange - fuzzy and C# parsers fail, LLM succeeds
    const string input = "day after tomorrow at 5pm";
    _ = _fuzzyTimeParser
      .Setup(p => p.TryParseFuzzyTime(input, ReferenceTime))
      .Returns(Result.Fail<DateTime>("Not fuzzy"));

    var mockBuilder = new Mock<IAIRequestBuilder>();
    _ = _aiAgent.Setup(a => a.CreateTimeParsingRequestAsync(input, "UTC", It.IsAny<string>()))
      .ReturnsAsync(mockBuilder.Object);
    _ = mockBuilder.Setup(b => b.ExecuteAsync(default))
      .ReturnsAsync(AIRequestResult.SuccessWithContent("2026-01-01T17:00:00"));

    // Act
    var result = await _service.ParseTimeAsync(input);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(new DateTime(2026, 1, 1, 17, 0, 0, DateTimeKind.Utc), result.Value);
  }

  [Fact]
  public async Task ParseTimeAsyncWhenLlmReturnsUnparseableReturnsFailureAsync()
  {
    // Arrange
    const string input = "buy groceries";
    _ = _fuzzyTimeParser
      .Setup(p => p.TryParseFuzzyTime(input, ReferenceTime))
      .Returns(Result.Fail<DateTime>("Not fuzzy"));

    var mockBuilder = new Mock<IAIRequestBuilder>();
    _ = _aiAgent.Setup(a => a.CreateTimeParsingRequestAsync(input, "UTC", It.IsAny<string>()))
      .ReturnsAsync(mockBuilder.Object);
    _ = mockBuilder.Setup(b => b.ExecuteAsync(default))
      .ReturnsAsync(AIRequestResult.SuccessWithContent("UNPARSEABLE"));

    // Act
    var result = await _service.ParseTimeAsync(input);

    // Assert
    Assert.True(result.IsFailed);
    Assert.Contains("Invalid time format", result.Errors[0].Message);
  }

  [Fact]
  public async Task ParseTimeAsyncWhenLlmReturnsInvalidFormatReturnsFailureAsync()
  {
    // Arrange
    const string input = "some weird expression";
    _ = _fuzzyTimeParser
      .Setup(p => p.TryParseFuzzyTime(input, ReferenceTime))
      .Returns(Result.Fail<DateTime>("Not fuzzy"));

    var mockBuilder = new Mock<IAIRequestBuilder>();
    _ = _aiAgent.Setup(a => a.CreateTimeParsingRequestAsync(input, "UTC", It.IsAny<string>()))
      .ReturnsAsync(mockBuilder.Object);
    _ = mockBuilder.Setup(b => b.ExecuteAsync(default))
      .ReturnsAsync(AIRequestResult.SuccessWithContent("not a valid date either"));

    // Act
    var result = await _service.ParseTimeAsync(input);

    // Assert
    Assert.True(result.IsFailed);
    Assert.Contains("Invalid time format", result.Errors[0].Message);
  }

  [Fact]
  public async Task ParseTimeAsyncWhenLlmFailsReturnsFailureAsync()
  {
    // Arrange
    const string input = "something complex";
    _ = _fuzzyTimeParser
      .Setup(p => p.TryParseFuzzyTime(input, ReferenceTime))
      .Returns(Result.Fail<DateTime>("Not fuzzy"));

    var mockBuilder = new Mock<IAIRequestBuilder>();
    _ = _aiAgent.Setup(a => a.CreateTimeParsingRequestAsync(input, "UTC", It.IsAny<string>()))
      .ReturnsAsync(mockBuilder.Object);
    _ = mockBuilder.Setup(b => b.ExecuteAsync(default))
      .ReturnsAsync(AIRequestResult.Failure("LLM error"));

    // Act
    var result = await _service.ParseTimeAsync(input);

    // Assert
    Assert.True(result.IsFailed);
    Assert.Contains("Invalid time format", result.Errors[0].Message);
  }

  [Fact]
  public async Task ParseTimeAsyncWithFuzzyTimeAndUserTimezoneConvertsToUtc()
  {
    // Arrange — fuzzy parser returns a time that should be treated as user-local
    // "tomorrow at 3pm" → fuzzy returns 2025-12-31T15:00:00 with Unspecified kind
    var fuzzyParsed = new DateTime(2025, 12, 31, 15, 0, 0, DateTimeKind.Unspecified);
    _fuzzyTimeParser
      .Setup(p => p.TryParseFuzzyTime("tomorrow at 3pm", ReferenceTime))
      .Returns(Result.Ok(fuzzyParsed));

    // Act — America/Chicago is UTC-6 in winter
    var result = await _service.ParseTimeAsync("tomorrow at 3pm", "America/Chicago");

    // Assert — 15:00 CST should convert to 21:00 UTC
    Assert.True(result.IsSuccess);
    Assert.Equal(new DateTime(2025, 12, 31, 21, 0, 0, DateTimeKind.Utc), result.Value);
  }

  [Fact]
  public async Task ParseTimeAsyncWithFuzzyTimeAndNoTimezoneReturnsUtcDirectly()
  {
    // Arrange — fuzzy parser returns UTC time (relative durations)
    var expected = new DateTime(2025, 12, 30, 14, 40, 0, DateTimeKind.Utc);
    _fuzzyTimeParser
      .Setup(p => p.TryParseFuzzyTime("in 10 minutes", ReferenceTime))
      .Returns(Result.Ok(expected));

    // Act — no timezone provided
    var result = await _service.ParseTimeAsync("in 10 minutes");

    // Assert — UTC time should pass through unchanged
    Assert.True(result.IsSuccess);
    Assert.Equal(expected, result.Value);
  }

  [Fact]
  public async Task ParseTimeAsyncWithIso8601ZSuffixReturnsUtcKind()
  {
    // Arrange — fuzzy parser fails
    const string input = "2025-12-31T10:00:00Z";
    _fuzzyTimeParser
      .Setup(p => p.TryParseFuzzyTime(input, ReferenceTime))
      .Returns(Result.Fail<DateTime>("Not fuzzy"));

    // Act
    var result = await _service.ParseTimeAsync(input);

    // Assert — Z suffix means UTC; should NOT be re-converted via timezone
    Assert.True(result.IsSuccess);
    Assert.Equal(new DateTime(2025, 12, 31, 10, 0, 0, DateTimeKind.Utc), result.Value);
  }

  [Fact]
  public async Task ParseTimeAsyncWithIso8601OffsetConvertsToUtc()
  {
    // Arrange — fuzzy parser fails
    const string input = "2025-12-31T10:00:00-05:00";
    _fuzzyTimeParser
      .Setup(p => p.TryParseFuzzyTime(input, ReferenceTime))
      .Returns(Result.Fail<DateTime>("Not fuzzy"));

    // Act
    var result = await _service.ParseTimeAsync(input);

    // Assert — 10:00 at -05:00 should convert to 15:00 UTC
    Assert.True(result.IsSuccess);
    Assert.Equal(new DateTime(2025, 12, 31, 15, 0, 0, DateTimeKind.Utc), result.Value);
  }

  [Fact]
  public async Task ParseTimeAsyncWithInvalidTimezoneIdFallsBackToUtc()
  {
    // Arrange — fuzzy parser fails, C# parsing succeeds with Unspecified kind
    const string input = "2025-12-31T10:00:00";
    _fuzzyTimeParser
      .Setup(p => p.TryParseFuzzyTime(input, ReferenceTime))
      .Returns(Result.Fail<DateTime>("Not fuzzy"));

    // Act — invalid timezone should not throw, should fallback to UTC
    var result = await _service.ParseTimeAsync(input, "Invalid/Timezone");

    // Assert — should succeed, treating as UTC since timezone is invalid
    Assert.True(result.IsSuccess);
    Assert.Equal(new DateTime(2025, 12, 31, 10, 0, 0, DateTimeKind.Utc), result.Value);
  }

  [Fact]
  public async Task ParseTimeAsyncWithEmptyTimezoneIdTreatsAsUtc()
  {
    // Arrange — fuzzy parser fails, C# parsing succeeds
    const string input = "2025-12-31T10:00:00";
    _fuzzyTimeParser
      .Setup(p => p.TryParseFuzzyTime(input, ReferenceTime))
      .Returns(Result.Fail<DateTime>("Not fuzzy"));

    // Act — empty string timezone should behave like null
    var result = await _service.ParseTimeAsync(input, "   ");

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(new DateTime(2025, 12, 31, 10, 0, 0, DateTimeKind.Utc), result.Value);
  }
}
