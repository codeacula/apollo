using Apollo.AI.Plugins;

namespace Apollo.AI.Tests.Plugins;

public class TimePluginTests
{
  private readonly TimeProvider _mockTimeProvider;
  private readonly TimePlugin _plugin;
  private readonly DateTimeOffset _fixedNow = new(2025, 12, 12, 14, 30, 0, TimeSpan.Zero);

  public TimePluginTests()
  {
    _mockTimeProvider = new FixedTimeProvider(_fixedNow);
    _plugin = new TimePlugin(_mockTimeProvider);
  }

  [Fact]
  public void GetDateReturnsCurrentDateInIsoFormat()
  {
    var result = _plugin.GetDate();
    Assert.Equal("2025-12-12", result);
  }

  [Fact]
  public void GetTimeReturnsCurrentTimeInIsoFormat()
  {
    var result = _plugin.GetTime();
    Assert.Equal("14:30:00", result);
  }

  [Fact]
  public void GetDateTimeReturnsCurrentDateTimeInIsoFormat()
  {
    var result = _plugin.GetDateTime();
    Assert.Equal("2025-12-12T14:30:00", result);
  }

  [Fact]
  public void ConvertToTimeZoneWithValidTimestampAndTimeZoneReturnsConvertedTime()
  {
    var result = _plugin.ConvertToTimeZone("2025-12-12T14:30:00Z", "America/New_York");
    Assert.NotNull(result);
    Assert.NotEmpty(result);
  }

  [Fact]
  public void ConvertToTimeZoneWithInvalidTimestampThrowsArgumentException()
  {
    var exception = Assert.Throws<ArgumentException>(() =>
      _plugin.ConvertToTimeZone("invalid-timestamp", "America/New_York"));
    Assert.Equal("Invalid timestamp format. (Parameter 'timestamp')", exception.Message);
  }

  [Fact]
  public void ConvertToTimeZoneWithInvalidTimeZoneIdThrowsTimeZoneNotFoundException()
  {
    _ = Assert.Throws<TimeZoneNotFoundException>(() =>
      _plugin.ConvertToTimeZone("2025-12-12T14:30:00Z", "Invalid/TimeZone"));
  }

  [Fact]
  public void GetFuzzyDateWithNullOrWhitespaceThrowsArgumentException()
  {
    var exception = Assert.Throws<ArgumentException>(() => _plugin.GetFuzzyDate(null!));
    Assert.Equal("Description cannot be empty. (Parameter 'description')", exception.Message);
  }

  [Fact]
  public void GetFuzzyDateWithEmptyStringThrowsArgumentException()
  {
    var exception = Assert.Throws<ArgumentException>(() => _plugin.GetFuzzyDate(""));
    Assert.Equal("Description cannot be empty. (Parameter 'description')", exception.Message);
  }

  [Fact]
  public void GetFuzzyDateWithWhitespaceOnlyThrowsArgumentException()
  {
    var exception = Assert.Throws<ArgumentException>(() => _plugin.GetFuzzyDate("   "));
    Assert.Equal("Description cannot be empty. (Parameter 'description')", exception.Message);
  }

  [Theory]
  [InlineData("now")]
  [InlineData("today")]
  public void GetFuzzyDateWithNowOrTodayReturnsCurrentDateTime(string input)
  {
    var result = _plugin.GetFuzzyDate(input);
    Assert.Equal("2025-12-12T14:30:00", result);
  }

  [Fact]
  public void GetFuzzyDateWithTomorrowReturnsNextDay()
  {
    var result = _plugin.GetFuzzyDate("tomorrow");
    Assert.Equal("2025-12-13T14:30:00", result);
  }

  [Fact]
  public void GetFuzzyDateWithYesterdayReturnsPreviousDay()
  {
    var result = _plugin.GetFuzzyDate("yesterday");
    Assert.Equal("2025-12-11T14:30:00", result);
  }

  [Fact]
  public void GetFuzzyDateWithNextWeekReturnsDateSevenDaysLater()
  {
    var result = _plugin.GetFuzzyDate("next week");
    Assert.Equal("2025-12-19T14:30:00", result);
  }

  [Fact]
  public void GetFuzzyDateWithNextMonthReturnsDateOneMonthLater()
  {
    var result = _plugin.GetFuzzyDate("next month");
    Assert.Equal("2026-01-12T14:30:00", result);
  }

  [Fact]
  public void GetFuzzyDateWithNextYearReturnsDateOneYearLater()
  {
    var result = _plugin.GetFuzzyDate("next year");
    Assert.Equal("2026-12-12T14:30:00", result);
  }

  [Fact]
  public void GetFuzzyDateWithInOneHourReturnsCorrectTimestamp()
  {
    var result = _plugin.GetFuzzyDate("in 1 hour");
    Assert.Equal("2025-12-12T15:30:00", result);
  }

  [Fact]
  public void GetFuzzyDateWithInOneHourFromNowReturnsCorrectTimestamp()
  {
    var result = _plugin.GetFuzzyDate("in 1 hour from now");
    Assert.Equal("2025-12-12T15:30:00", result);
  }

  [Fact]
  public void GetFuzzyDateWithInMultipleHoursReturnsCorrectTimestamp()
  {
    var result = _plugin.GetFuzzyDate("in 3 hours");
    Assert.Equal("2025-12-12T17:30:00", result);
  }

  [Fact]
  public void GetFuzzyDateWithInMinutesReturnsCorrectTimestamp()
  {
    var result = _plugin.GetFuzzyDate("in 45 minutes");
    Assert.Equal("2025-12-12T15:15:00", result);
  }

  [Fact]
  public void GetFuzzyDateWithInSecondsReturnsCorrectTimestamp()
  {
    var result = _plugin.GetFuzzyDate("in 30 seconds");
    Assert.Equal("2025-12-12T14:30:30", result);
  }

  [Fact]
  public void GetFuzzyDateWithInDaysReturnsCorrectTimestamp()
  {
    var result = _plugin.GetFuzzyDate("in 5 days");
    Assert.Equal("2025-12-17T14:30:00", result);
  }

  [Fact]
  public void GetFuzzyDateWithInWeeksReturnsCorrectTimestamp()
  {
    var result = _plugin.GetFuzzyDate("in 2 weeks");
    Assert.Equal("2025-12-26T14:30:00", result);
  }

  [Fact]
  public void GetFuzzyDateWithInMonthsReturnsCorrectTimestamp()
  {
    var result = _plugin.GetFuzzyDate("in 3 months");
    Assert.Equal("2026-03-12T14:30:00", result);
  }

  [Fact]
  public void GetFuzzyDateWithInYearsReturnsCorrectTimestamp()
  {
    var result = _plugin.GetFuzzyDate("in 2 years");
    Assert.Equal("2027-12-12T14:30:00", result);
  }

  [Fact]
  public void GetFuzzyDateWithMixedCaseReturnsCorrectTimestamp()
  {
    var result = _plugin.GetFuzzyDate("In 1 Hour");
    Assert.Equal("2025-12-12T15:30:00", result);
  }

  [Fact]
  public void GetFuzzyDateWithLeadingAndTrailingWhitespaceReturnsCorrectTimestamp()
  {
    var result = _plugin.GetFuzzyDate("  in 2 hours  ");
    Assert.Equal("2025-12-12T16:30:00", result);
  }

  [Fact]
  public void GetFuzzyDateWithValidIsoDateStringParsesAndReturnsTimestamp()
  {
    var result = _plugin.GetFuzzyDate("2025-12-25T12:00:00");
    Assert.Equal("2025-12-25T12:00:00", result);
  }

  [Fact]
  public void GetFuzzyDateWithUnparsableDescriptionThrowsArgumentException()
  {
    var exception = Assert.Throws<ArgumentException>(() =>
      _plugin.GetFuzzyDate("some random text"));
    Assert.Equal("Unable to parse the provided description. (Parameter 'description')", exception.Message);
  }

  [Fact]
  public void GetFuzzyDateWithSingularUnitsReturnsCorrectTimestamp()
  {
    var result = _plugin.GetFuzzyDate("in 1 hour");
    Assert.Equal("2025-12-12T15:30:00", result);
  }

  [Fact]
  public void GetFuzzyDateWithPluralUnitsReturnsCorrectTimestamp()
  {
    var result = _plugin.GetFuzzyDate("in 2 hours");
    Assert.Equal("2025-12-12T16:30:00", result);
  }
  private sealed class FixedTimeProvider(DateTimeOffset fixedTime) : TimeProvider
  {
    private readonly DateTimeOffset _fixedTime = fixedTime;

    public override DateTimeOffset GetUtcNow() => _fixedTime;
  }
}
