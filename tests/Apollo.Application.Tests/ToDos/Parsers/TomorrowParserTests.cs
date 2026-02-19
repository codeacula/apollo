using Apollo.Application.ToDos.Parsers;

namespace Apollo.Application.Tests.ToDos.Parsers;

public class TomorrowParserTests
{
  private readonly TomorrowParser _parser = new();
  private readonly DateTime _ref = new(2025, 12, 30, 14, 30, 0, DateTimeKind.Utc);

  [Fact]
  public void TryParseWithTomorrowReturnsTomorrow()
  {
    var result = _parser.TryParse("tomorrow", _ref);

    Assert.True(result.IsSuccess);
    Assert.Equal(_ref.AddDays(1), result.Value);
    Assert.Equal(DateTimeKind.Utc, result.Value.Kind);
  }

  [Theory]
  [InlineData("TOMORROW")]
  [InlineData("  tomorrow  ")]
  [InlineData("Tomorrow")]
  public void TryParseWithTomorrowCaseInsensitivelyReturnsCorrectDate(string input)
  {
    var result = _parser.TryParse(input, _ref);

    Assert.True(result.IsSuccess);
    Assert.Equal(_ref.AddDays(1), result.Value);
  }

  [Theory]
  [InlineData("tomorrow at 3pm", 15, 0)]
  [InlineData("tomorrow at 3:00pm", 15, 0)]
  [InlineData("tomorrow at 15:00", 15, 0)]
  [InlineData("tomorrow at 9am", 9, 0)]
  [InlineData("tomorrow at 12:30pm", 12, 30)]
  [InlineData("TOMORROW AT 3PM", 15, 0)]
  public void TryParseWithTomorrowAtTimeReturnsCorrectDateTime(string input, int hour, int minute)
  {
    var result = _parser.TryParse(input, _ref);

    Assert.True(result.IsSuccess);
    var expected = new DateTime(2025, 12, 31, hour, minute, 0, DateTimeKind.Utc);
    Assert.Equal(expected, result.Value);
    Assert.Equal(DateTimeKind.Utc, result.Value.Kind);
  }

  [Theory]
  [InlineData("at 3pm")]
  [InlineData("next Monday")]
  [InlineData("tonight")]
  [InlineData("in 10 minutes")]
  public void TryParseWithNonTomorrowInputFails(string input)
  {
    var result = _parser.TryParse(input, _ref);

    Assert.True(result.IsFailed);
  }
}
