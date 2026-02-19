using Apollo.Application.ToDos.Parsers;

namespace Apollo.Application.Tests.ToDos.Parsers;

public class DayOfWeekParserTests
{
  private readonly DayOfWeekParser _parser = new();
  /// <summary>
  /// Reference: Tuesday, Dec 30, 2025
  /// </summary>
  private readonly DateTime _ref = new(2025, 12, 30, 14, 30, 0, DateTimeKind.Utc);

  [Fact]
  public void TryParseWithNextWeekReturnsSevenDaysFromNow()
  {
    var result = _parser.TryParse("next week", _ref);

    Assert.True(result.IsSuccess);
    Assert.Equal(_ref.AddDays(7), result.Value);
    Assert.Equal(DateTimeKind.Utc, result.Value.Kind);
  }

  [Theory]
  [InlineData("next Monday", DayOfWeek.Monday)]
  [InlineData("next Friday", DayOfWeek.Friday)]
  [InlineData("next Sunday", DayOfWeek.Sunday)]
  [InlineData("NEXT TUESDAY", DayOfWeek.Tuesday)]
  public void TryParseWithNextDayOfWeekReturnsCorrectDay(string input, DayOfWeek expectedDay)
  {
    var result = _parser.TryParse(input, _ref);

    Assert.True(result.IsSuccess);
    Assert.Equal(expectedDay, result.Value.DayOfWeek);
    Assert.True(result.Value > _ref.Date);
    Assert.Equal(DateTimeKind.Utc, result.Value.Kind);
  }

  [Fact]
  public void TryParseWithNextSameDayReturnsFollowingWeek()
  {
    // Reference is Tuesday; "next Tuesday" should jump to Jan 6, 2026
    var result = _parser.TryParse("next Tuesday", _ref);

    Assert.True(result.IsSuccess);
    Assert.Equal(new DateTime(2026, 1, 6, 0, 0, 0, DateTimeKind.Utc), result.Value);
  }

  [Theory]
  [InlineData("on Tuesday", DayOfWeek.Tuesday)]
  [InlineData("on Wednesday", DayOfWeek.Wednesday)]
  [InlineData("ON FRIDAY", DayOfWeek.Friday)]
  public void TryParseWithOnDayOfWeekReturnsCorrectDay(string input, DayOfWeek expectedDay)
  {
    var result = _parser.TryParse(input, _ref);

    Assert.True(result.IsSuccess);
    Assert.Equal(expectedDay, result.Value.DayOfWeek);
    Assert.True(result.Value > _ref.Date);
    Assert.Equal(DateTimeKind.Utc, result.Value.Kind);
  }

  [Theory]
  [InlineData("on Friday at 3pm", DayOfWeek.Friday, 15, 0)]
  [InlineData("next Monday at 9am", DayOfWeek.Monday, 9, 0)]
  public void TryParseWithDayAtTimeReturnsCorrectDateTime(
    string input, DayOfWeek expectedDay, int expectedHour, int expectedMinute)
  {
    var result = _parser.TryParse(input, _ref);

    Assert.True(result.IsSuccess);
    Assert.Equal(expectedDay, result.Value.DayOfWeek);
    Assert.Equal(expectedHour, result.Value.Hour);
    Assert.Equal(expectedMinute, result.Value.Minute);
    Assert.Equal(DateTimeKind.Utc, result.Value.Kind);
  }

  [Theory]
  [InlineData("tomorrow")]
  [InlineData("in 10 minutes")]
  [InlineData("tonight")]
  [InlineData("end of day")]
  public void TryParseWithNonDayOfWeekInputFails(string input)
  {
    var result = _parser.TryParse(input, _ref);

    Assert.True(result.IsFailed);
  }
}
