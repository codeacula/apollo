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

  // ===== NEW PATTERN TESTS =====

  [Theory]
  [InlineData("tomorrow at 3pm", 15, 0)]
  [InlineData("tomorrow at 3:00pm", 15, 0)]
  [InlineData("tomorrow at 15:00", 15, 0)]
  [InlineData("tomorrow at 9am", 9, 0)]
  [InlineData("tomorrow at 12:30pm", 12, 30)]
  [InlineData("TOMORROW AT 3PM", 15, 0)]
  public void TryParseFuzzyTimeWithTomorrowAtTimeReturnsCorrectDateTime(
    string input, int expectedHour, int expectedMinute)
  {
    var result = _parser.TryParseFuzzyTime(input, _referenceTime);

    Assert.True(result.IsSuccess);
    // Reference is 2025-12-30, so tomorrow is 2025-12-31
    var expected = new DateTime(2025, 12, 31, expectedHour, expectedMinute, 0, DateTimeKind.Utc);
    Assert.Equal(expected, result.Value);
    Assert.Equal(DateTimeKind.Utc, result.Value.Kind);
  }

  [Theory]
  [InlineData("at 3pm", 15, 0)]
  [InlineData("at 3:00pm", 15, 0)]
  [InlineData("at 15:00", 15, 0)]
  [InlineData("at 9am", 9, 0)]
  [InlineData("AT 3PM", 15, 0)]
  public void TryParseFuzzyTimeWithAtTimeReturnsTodayAtTime(
    string input, int expectedHour, int expectedMinute)
  {
    var result = _parser.TryParseFuzzyTime(input, _referenceTime);

    Assert.True(result.IsSuccess);
    // Reference date is 2025-12-30
    var expected = new DateTime(2025, 12, 30, expectedHour, expectedMinute, 0, DateTimeKind.Utc);
    Assert.Equal(expected, result.Value);
    Assert.Equal(DateTimeKind.Utc, result.Value.Kind);
  }

  [Fact]
  public void TryParseFuzzyTimeWithAtNoonReturnsTodayAtNoon()
  {
    var result = _parser.TryParseFuzzyTime("at noon", _referenceTime);

    Assert.True(result.IsSuccess);
    var expected = new DateTime(2025, 12, 30, 12, 0, 0, DateTimeKind.Utc);
    Assert.Equal(expected, result.Value);
  }

  [Fact]
  public void TryParseFuzzyTimeWithAtMidnightReturnsTomorrowMidnight()
  {
    var result = _parser.TryParseFuzzyTime("at midnight", _referenceTime);

    Assert.True(result.IsSuccess);
    var expected = new DateTime(2025, 12, 31, 0, 0, 0, DateTimeKind.Utc);
    Assert.Equal(expected, result.Value);
  }

  [Fact]
  public void TryParseFuzzyTimeWithTonightReturnsEveningTime()
  {
    var result = _parser.TryParseFuzzyTime("tonight", _referenceTime);

    Assert.True(result.IsSuccess);
    // tonight = 8pm (20:00) on reference date
    var expected = new DateTime(2025, 12, 30, 20, 0, 0, DateTimeKind.Utc);
    Assert.Equal(expected, result.Value);
    Assert.Equal(DateTimeKind.Utc, result.Value.Kind);
  }

  [Fact]
  public void TryParseFuzzyTimeWithThisMorningReturnsMorningTime()
  {
    var result = _parser.TryParseFuzzyTime("this morning", _referenceTime);

    Assert.True(result.IsSuccess);
    // this morning = 9am on reference date
    var expected = new DateTime(2025, 12, 30, 9, 0, 0, DateTimeKind.Utc);
    Assert.Equal(expected, result.Value);
    Assert.Equal(DateTimeKind.Utc, result.Value.Kind);
  }

  [Fact]
  public void TryParseFuzzyTimeWithThisAfternoonReturnsAfternoonTime()
  {
    var result = _parser.TryParseFuzzyTime("this afternoon", _referenceTime);

    Assert.True(result.IsSuccess);
    // this afternoon = 2pm on reference date
    var expected = new DateTime(2025, 12, 30, 14, 0, 0, DateTimeKind.Utc);
    Assert.Equal(expected, result.Value);
    Assert.Equal(DateTimeKind.Utc, result.Value.Kind);
  }

  [Fact]
  public void TryParseFuzzyTimeWithThisEveningReturnsEveningTime()
  {
    var result = _parser.TryParseFuzzyTime("this evening", _referenceTime);

    Assert.True(result.IsSuccess);
    // this evening = 6pm on reference date
    var expected = new DateTime(2025, 12, 30, 18, 0, 0, DateTimeKind.Utc);
    Assert.Equal(expected, result.Value);
    Assert.Equal(DateTimeKind.Utc, result.Value.Kind);
  }

  [Fact]
  public void TryParseFuzzyTimeWithNoonReturnsTodayAtNoon()
  {
    var result = _parser.TryParseFuzzyTime("noon", _referenceTime);

    Assert.True(result.IsSuccess);
    var expected = new DateTime(2025, 12, 30, 12, 0, 0, DateTimeKind.Utc);
    Assert.Equal(expected, result.Value);
    Assert.Equal(DateTimeKind.Utc, result.Value.Kind);
  }

  [Fact]
  public void TryParseFuzzyTimeWithMidnightReturnsTomorrowMidnight()
  {
    var result = _parser.TryParseFuzzyTime("midnight", _referenceTime);

    Assert.True(result.IsSuccess);
    // midnight = start of next day
    var expected = new DateTime(2025, 12, 31, 0, 0, 0, DateTimeKind.Utc);
    Assert.Equal(expected, result.Value);
    Assert.Equal(DateTimeKind.Utc, result.Value.Kind);
  }

  [Theory]
  [InlineData("next Monday", DayOfWeek.Monday)]
  [InlineData("next Friday", DayOfWeek.Friday)]
  [InlineData("next Sunday", DayOfWeek.Sunday)]
  [InlineData("NEXT TUESDAY", DayOfWeek.Tuesday)]
  public void TryParseFuzzyTimeWithNextDayOfWeekReturnsCorrectDate(string input, DayOfWeek expectedDay)
  {
    // Reference is Tuesday Dec 30, 2025
    var result = _parser.TryParseFuzzyTime(input, _referenceTime);

    Assert.True(result.IsSuccess);
    Assert.Equal(expectedDay, result.Value.DayOfWeek);
    Assert.True(result.Value > _referenceTime.Date);
    Assert.Equal(DateTimeKind.Utc, result.Value.Kind);
  }

  [Theory]
  [InlineData("on Tuesday", DayOfWeek.Tuesday)]
  [InlineData("on Wednesday", DayOfWeek.Wednesday)]
  [InlineData("ON FRIDAY", DayOfWeek.Friday)]
  public void TryParseFuzzyTimeWithOnDayOfWeekReturnsCorrectDate(string input, DayOfWeek expectedDay)
  {
    var result = _parser.TryParseFuzzyTime(input, _referenceTime);

    Assert.True(result.IsSuccess);
    Assert.Equal(expectedDay, result.Value.DayOfWeek);
    Assert.True(result.Value > _referenceTime.Date);
    Assert.Equal(DateTimeKind.Utc, result.Value.Kind);
  }

  [Theory]
  [InlineData("on Friday at 3pm", DayOfWeek.Friday, 15, 0)]
  [InlineData("next Monday at 9am", DayOfWeek.Monday, 9, 0)]
  public void TryParseFuzzyTimeWithDayOfWeekAtTimeReturnsCorrectDateTime(
    string input, DayOfWeek expectedDay, int expectedHour, int expectedMinute)
  {
    var result = _parser.TryParseFuzzyTime(input, _referenceTime);

    Assert.True(result.IsSuccess);
    Assert.Equal(expectedDay, result.Value.DayOfWeek);
    Assert.Equal(expectedHour, result.Value.Hour);
    Assert.Equal(expectedMinute, result.Value.Minute);
    Assert.True(result.Value > _referenceTime.Date);
    Assert.Equal(DateTimeKind.Utc, result.Value.Kind);
  }

  [Fact]
  public void TryParseFuzzyTimeWithInAnHourReturnsOneHourFromNow()
  {
    var result = _parser.TryParseFuzzyTime("in an hour", _referenceTime);

    Assert.True(result.IsSuccess);
    Assert.Equal(_referenceTime.AddHours(1), result.Value);
    Assert.Equal(DateTimeKind.Utc, result.Value.Kind);
  }

  [Fact]
  public void TryParseFuzzyTimeWithInAHourReturnsOneHourFromNow()
  {
    var result = _parser.TryParseFuzzyTime("in a hour", _referenceTime);

    Assert.True(result.IsSuccess);
    Assert.Equal(_referenceTime.AddHours(1), result.Value);
    Assert.Equal(DateTimeKind.Utc, result.Value.Kind);
  }

  [Fact]
  public void TryParseFuzzyTimeWithInHalfAnHourReturnsThirtyMinutesFromNow()
  {
    var result = _parser.TryParseFuzzyTime("in half an hour", _referenceTime);

    Assert.True(result.IsSuccess);
    Assert.Equal(_referenceTime.AddMinutes(30), result.Value);
    Assert.Equal(DateTimeKind.Utc, result.Value.Kind);
  }

  [Theory]
  [InlineData("5 minutes", 5)]
  [InlineData("30 minutes", 30)]
  [InlineData("2 hours", 120)]
  [InlineData("1 hour", 60)]
  public void TryParseFuzzyTimeWithDurationWithoutPrefixReturnsFutureDate(string input, int expectedMinutes)
  {
    var result = _parser.TryParseFuzzyTime(input, _referenceTime);

    Assert.True(result.IsSuccess);
    Assert.Equal(_referenceTime.AddMinutes(expectedMinutes), result.Value);
    Assert.Equal(DateTimeKind.Utc, result.Value.Kind);
  }

  [Theory]
  [InlineData("end of day")]
  [InlineData("eod")]
  [InlineData("EOD")]
  [InlineData("End Of Day")]
  public void TryParseFuzzyTimeWithEndOfDayReturnsEndOfDay(string input)
  {
    var result = _parser.TryParseFuzzyTime(input, _referenceTime);

    Assert.True(result.IsSuccess);
    // end of day = 5pm on reference date
    var expected = new DateTime(2025, 12, 30, 17, 0, 0, DateTimeKind.Utc);
    Assert.Equal(expected, result.Value);
    Assert.Equal(DateTimeKind.Utc, result.Value.Kind);
  }

  [Fact]
  public void TryParseFuzzyTimeWithEndOfWeekReturnsEndOfWeek()
  {
    var result = _parser.TryParseFuzzyTime("end of week", _referenceTime);

    Assert.True(result.IsSuccess);
    // Reference is Tuesday Dec 30. Next Friday is Jan 2, 2026 at 5pm.
    Assert.Equal(DayOfWeek.Friday, result.Value.DayOfWeek);
    Assert.Equal(17, result.Value.Hour);
    Assert.Equal(DateTimeKind.Utc, result.Value.Kind);
  }

  [Fact]
  public void TryParseFuzzyTimeWithEndOfWeekOnFridayReturnsNextFriday()
  {
    // If today is Friday, "end of week" should return next Friday
    var fridayRef = new DateTime(2026, 1, 2, 10, 0, 0, DateTimeKind.Utc); // Friday

    var result = _parser.TryParseFuzzyTime("end of week", fridayRef);

    Assert.True(result.IsSuccess);
    Assert.Equal(DayOfWeek.Friday, result.Value.DayOfWeek);
    Assert.True(result.Value > fridayRef);
  }

  [Fact]
  public void TryParseFuzzyTimeWithNextDayOfWeekSameDayReturnsNextWeek()
  {
    // Reference is Tuesday Dec 30, 2025. "next Tuesday" should be Jan 6, 2026.
    var result = _parser.TryParseFuzzyTime("next Tuesday", _referenceTime);

    Assert.True(result.IsSuccess);
    Assert.Equal(DayOfWeek.Tuesday, result.Value.DayOfWeek);
    var expected = new DateTime(2026, 1, 6, 0, 0, 0, DateTimeKind.Utc);
    Assert.Equal(expected, result.Value);
  }
}
