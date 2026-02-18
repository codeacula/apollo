using Apollo.Application.ToDos.Parsers;

namespace Apollo.Application.Tests.ToDos.Parsers;

public class ClockTimeParserTests
{
  private readonly ClockTimeParser _parser = new();
  private readonly DateTime _ref = new(2025, 12, 30, 14, 30, 0, DateTimeKind.Utc);

  [Theory]
  [InlineData("at 3pm", 15, 0)]
  [InlineData("at 3:00pm", 15, 0)]
  [InlineData("at 15:00", 15, 0)]
  [InlineData("at 9am", 9, 0)]
  [InlineData("AT 3PM", 15, 0)]
  public void TryParseWithAtTimeReturnsTodayAtTime(string input, int hour, int minute)
  {
    var result = _parser.TryParse(input, _ref);

    Assert.True(result.IsSuccess);
    var expected = new DateTime(2025, 12, 30, hour, minute, 0, DateTimeKind.Utc);
    Assert.Equal(expected, result.Value);
    Assert.Equal(DateTimeKind.Utc, result.Value.Kind);
  }

  [Fact]
  public void TryParseWithAtNoonReturnsTodayNoon()
  {
    var result = _parser.TryParse("at noon", _ref);

    Assert.True(result.IsSuccess);
    Assert.Equal(new DateTime(2025, 12, 30, 12, 0, 0, DateTimeKind.Utc), result.Value);
  }

  [Fact]
  public void TryParseWithAtMidnightReturnsStartOfNextDay()
  {
    var result = _parser.TryParse("at midnight", _ref);

    Assert.True(result.IsSuccess);
    Assert.Equal(new DateTime(2025, 12, 31, 0, 0, 0, DateTimeKind.Utc), result.Value);
  }

  [Fact]
  public void TryParseWithStandaloneNoonReturnsTodayNoon()
  {
    var result = _parser.TryParse("noon", _ref);

    Assert.True(result.IsSuccess);
    Assert.Equal(new DateTime(2025, 12, 30, 12, 0, 0, DateTimeKind.Utc), result.Value);
  }

  [Fact]
  public void TryParseWithStandaloneMidnightReturnsStartOfNextDay()
  {
    var result = _parser.TryParse("midnight", _ref);

    Assert.True(result.IsSuccess);
    Assert.Equal(new DateTime(2025, 12, 31, 0, 0, 0, DateTimeKind.Utc), result.Value);
  }

  [Theory]
  [InlineData("tomorrow")]
  [InlineData("in 10 minutes")]
  [InlineData("next Monday")]
  [InlineData("tonight")]
  public void TryParseWithNonClockTimeInputFails(string input)
  {
    var result = _parser.TryParse(input, _ref);

    Assert.True(result.IsFailed);
  }
}
