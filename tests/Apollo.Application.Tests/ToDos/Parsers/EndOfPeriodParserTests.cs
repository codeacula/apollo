using Apollo.Application.ToDos.Parsers;

namespace Apollo.Application.Tests.ToDos.Parsers;

public class EndOfPeriodParserTests
{
  private readonly EndOfPeriodParser _parser = new();
  private readonly DateTime _ref = new(2025, 12, 30, 14, 30, 0, DateTimeKind.Utc); // Reference: Tuesday, Dec 30, 2025

  [Theory]
  [InlineData("end of day")]
  [InlineData("eod")]
  [InlineData("EOD")]
  [InlineData("End Of Day")]
  public void TryParseWithEndOfDayReturns5pm(string input)
  {
    var result = _parser.TryParse(input, _ref);

    Assert.True(result.IsSuccess);
    Assert.Equal(new DateTime(2025, 12, 30, 17, 0, 0, DateTimeKind.Utc), result.Value);
    Assert.Equal(DateTimeKind.Utc, result.Value.Kind);
  }

  [Fact]
  public void TryParseWithEndOfWeekReturnsFriday5pm()
  {
    var result = _parser.TryParse("end of week", _ref);

    Assert.True(result.IsSuccess);
    Assert.Equal(DayOfWeek.Friday, result.Value.DayOfWeek);
    Assert.Equal(17, result.Value.Hour);
    Assert.Equal(DateTimeKind.Utc, result.Value.Kind);
  }

  [Fact]
  public void TryParseWithEndOfWeekOnFridayReturnsNextFriday()
  {
    var friday = new DateTime(2026, 1, 2, 10, 0, 0, DateTimeKind.Utc);

    var result = _parser.TryParse("end of week", friday);

    Assert.True(result.IsSuccess);
    Assert.Equal(DayOfWeek.Friday, result.Value.DayOfWeek);
    Assert.True(result.Value > friday);
  }

  [Theory]
  [InlineData("tomorrow")]
  [InlineData("in 10 minutes")]
  [InlineData("tonight")]
  [InlineData("next Monday")]
  public void TryParseWithNonEndOfPeriodInputFails(string input)
  {
    var result = _parser.TryParse(input, _ref);

    Assert.True(result.IsFailed);
  }
}
