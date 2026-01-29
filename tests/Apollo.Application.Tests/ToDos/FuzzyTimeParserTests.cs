using Apollo.Application.ToDos;

namespace Apollo.Application.Tests.ToDos;

public class FuzzyTimeParserTests
{
  private readonly FuzzyTimeParser _parser = new();
  private readonly DateTime _referenceTime = new(2025, 12, 30, 14, 30, 0, DateTimeKind.Utc);

  [Theory]
  [InlineData("in 10 minutes", 10)]
  [InlineData("in 1 minute", 1)]
  [InlineData("in 30 min", 30)]
  [InlineData("in 5 mins", 5)]
  [InlineData("in 15m", 15)]
  [InlineData("IN 20 MINUTES", 20)]
  [InlineData("  in 10 minutes  ", 10)]
  public void TryParseFuzzyTimeParsesMinutesCorrectly(string input, int expectedMinutes)
  {
    var result = _parser.TryParseFuzzyTime(input, _referenceTime);

    Assert.True(result.IsSuccess);
    Assert.Equal(_referenceTime.AddMinutes(expectedMinutes), result.Value);
    Assert.Equal(DateTimeKind.Utc, result.Value.Kind);
  }

  [Theory]
  [InlineData("in 2 hours", 2)]
  [InlineData("in 1 hour", 1)]
  [InlineData("in 3 hr", 3)]
  [InlineData("in 4 hrs", 4)]
  [InlineData("in 5h", 5)]
  [InlineData("IN 6 HOURS", 6)]
  public void TryParseFuzzyTimeParsesHoursCorrectly(string input, int expectedHours)
  {
    var result = _parser.TryParseFuzzyTime(input, _referenceTime);

    Assert.True(result.IsSuccess);
    Assert.Equal(_referenceTime.AddHours(expectedHours), result.Value);
    Assert.Equal(DateTimeKind.Utc, result.Value.Kind);
  }

  [Theory]
  [InlineData("in 1 day", 1)]
  [InlineData("in 3 days", 3)]
  [InlineData("in 7d", 7)]
  public void TryParseFuzzyTimeParsesDaysCorrectly(string input, int expectedDays)
  {
    var result = _parser.TryParseFuzzyTime(input, _referenceTime);

    Assert.True(result.IsSuccess);
    Assert.Equal(_referenceTime.AddDays(expectedDays), result.Value);
    Assert.Equal(DateTimeKind.Utc, result.Value.Kind);
  }

  [Theory]
  [InlineData("in 1 week", 7)]
  [InlineData("in 2 weeks", 14)]
  [InlineData("in 1w", 7)]
  public void TryParseFuzzyTimeParsesWeeksCorrectly(string input, int expectedDays)
  {
    var result = _parser.TryParseFuzzyTime(input, _referenceTime);

    Assert.True(result.IsSuccess);
    Assert.Equal(_referenceTime.AddDays(expectedDays), result.Value);
    Assert.Equal(DateTimeKind.Utc, result.Value.Kind);
  }

  [Fact]
  public void TryParseFuzzyTimeParsesTomorrowCorrectly()
  {
    var result = _parser.TryParseFuzzyTime("tomorrow", _referenceTime);

    Assert.True(result.IsSuccess);
    Assert.Equal(_referenceTime.AddDays(1), result.Value);
    Assert.Equal(DateTimeKind.Utc, result.Value.Kind);
  }

  [Theory]
  [InlineData("TOMORROW")]
  [InlineData("  tomorrow  ")]
  [InlineData("Tomorrow")]
  public void TryParseFuzzyTimeParsesTomorrowCaseInsensitively(string input)
  {
    var result = _parser.TryParseFuzzyTime(input, _referenceTime);

    Assert.True(result.IsSuccess);
    Assert.Equal(_referenceTime.AddDays(1), result.Value);
  }

  [Fact]
  public void TryParseFuzzyTimeParsesNextWeekCorrectly()
  {
    var result = _parser.TryParseFuzzyTime("next week", _referenceTime);

    Assert.True(result.IsSuccess);
    Assert.Equal(_referenceTime.AddDays(7), result.Value);
    Assert.Equal(DateTimeKind.Utc, result.Value.Kind);
  }

  [Theory]
  [InlineData("NEXT WEEK")]
  [InlineData("  next week  ")]
  [InlineData("Next Week")]
  public void TryParseFuzzyTimeParsesNextWeekCaseInsensitively(string input)
  {
    var result = _parser.TryParseFuzzyTime(input, _referenceTime);

    Assert.True(result.IsSuccess);
    Assert.Equal(_referenceTime.AddDays(7), result.Value);
  }

  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  [InlineData(null)]
  public void TryParseFuzzyTimeFailsForEmptyInput(string? input)
  {
    var result = _parser.TryParseFuzzyTime(input!, _referenceTime);

    Assert.True(result.IsFailed);
  }

  [Theory]
  [InlineData("2025-12-31T10:00:00")]
  [InlineData("hello world")]
  [InlineData("10 minutes")]
  [InlineData("in minutes")]
  [InlineData("yesterday")]
  [InlineData("last week")]
  public void TryParseFuzzyTimeFailsForNonFuzzyInput(string input)
  {
    var result = _parser.TryParseFuzzyTime(input, _referenceTime);

    Assert.True(result.IsFailed);
  }

  [Fact]
  public void TryParseFuzzyTimePreservesUtcKindFromReference()
  {
    var utcReference = new DateTime(2025, 12, 30, 12, 0, 0, DateTimeKind.Utc);

    var result = _parser.TryParseFuzzyTime("in 10 minutes", utcReference);

    Assert.True(result.IsSuccess);
    Assert.Equal(DateTimeKind.Utc, result.Value.Kind);
  }

  [Fact]
  public void TryParseFuzzyTimeConvertsUnspecifiedKindToUtc()
  {
    var unspecifiedReference = new DateTime(2025, 12, 30, 12, 0, 0, DateTimeKind.Unspecified);

    var result = _parser.TryParseFuzzyTime("in 10 minutes", unspecifiedReference);

    Assert.True(result.IsSuccess);
    Assert.Equal(DateTimeKind.Utc, result.Value.Kind);
  }
}
