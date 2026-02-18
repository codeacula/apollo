using Apollo.Application.ToDos.Parsers;

namespace Apollo.Application.Tests.ToDos.Parsers;

public class DurationParserTests
{
  private readonly DurationParser _parser = new();
  private readonly DateTime _ref = new(2025, 12, 30, 14, 30, 0, DateTimeKind.Utc);

  [Theory]
  [InlineData("in 10 minutes", 10)]
  [InlineData("in 1 minute", 1)]
  [InlineData("in 30 min", 30)]
  [InlineData("in 5 mins", 5)]
  [InlineData("in 15m", 15)]
  [InlineData("IN 20 MINUTES", 20)]
  [InlineData("  in 10 minutes  ", 10)]
  public void TryParseWithInMinutesReturnsCorrectDateTime(string input, int minutes)
  {
    var result = _parser.TryParse(input, _ref);

    Assert.True(result.IsSuccess);
    Assert.Equal(_ref.AddMinutes(minutes), result.Value);
    Assert.Equal(DateTimeKind.Utc, result.Value.Kind);
  }

  [Theory]
  [InlineData("in 2 hours", 2)]
  [InlineData("in 1 hour", 1)]
  [InlineData("in 3 hr", 3)]
  [InlineData("in 5h", 5)]
  public void TryParseWithInHoursReturnsCorrectDateTime(string input, int hours)
  {
    var result = _parser.TryParse(input, _ref);

    Assert.True(result.IsSuccess);
    Assert.Equal(_ref.AddHours(hours), result.Value);
    Assert.Equal(DateTimeKind.Utc, result.Value.Kind);
  }

  [Theory]
  [InlineData("in 1 day", 1)]
  [InlineData("in 3 days", 3)]
  [InlineData("in 7d", 7)]
  public void TryParseWithInDaysReturnsCorrectDateTime(string input, int days)
  {
    var result = _parser.TryParse(input, _ref);

    Assert.True(result.IsSuccess);
    Assert.Equal(_ref.AddDays(days), result.Value);
    Assert.Equal(DateTimeKind.Utc, result.Value.Kind);
  }

  [Theory]
  [InlineData("in 1 week", 7)]
  [InlineData("in 2 weeks", 14)]
  [InlineData("in 1w", 7)]
  public void TryParseWithInWeeksReturnsCorrectDays(string input, int days)
  {
    var result = _parser.TryParse(input, _ref);

    Assert.True(result.IsSuccess);
    Assert.Equal(_ref.AddDays(days), result.Value);
    Assert.Equal(DateTimeKind.Utc, result.Value.Kind);
  }

  [Fact]
  public void TryParseWithInAnHourReturnsOneHourFromNow()
  {
    var result = _parser.TryParse("in an hour", _ref);

    Assert.True(result.IsSuccess);
    Assert.Equal(_ref.AddHours(1), result.Value);
  }

  [Fact]
  public void TryParseWithInAHourReturnsOneHourFromNow()
  {
    var result = _parser.TryParse("in a hour", _ref);

    Assert.True(result.IsSuccess);
    Assert.Equal(_ref.AddHours(1), result.Value);
  }

  [Fact]
  public void TryParseWithInHalfAnHourReturnsThirtyMinutes()
  {
    var result = _parser.TryParse("in half an hour", _ref);

    Assert.True(result.IsSuccess);
    Assert.Equal(_ref.AddMinutes(30), result.Value);
  }

  [Theory]
  [InlineData("5 minutes", 5)]
  [InlineData("30 minutes", 30)]
  [InlineData("2 hours", 120)]
  [InlineData("1 hour", 60)]
  public void TryParseWithBareNumberDurationReturnsFutureDateTime(string input, int expectedMinutes)
  {
    var result = _parser.TryParse(input, _ref);

    Assert.True(result.IsSuccess);
    Assert.Equal(_ref.AddMinutes(expectedMinutes), result.Value);
  }

  [Theory]
  [InlineData("tomorrow")]
  [InlineData("next Monday")]
  [InlineData("at 3pm")]
  [InlineData("hello world")]
  public void TryParseWithNonDurationInputFails(string input)
  {
    var result = _parser.TryParse(input, _ref);

    Assert.True(result.IsFailed);
  }
}
