using Apollo.Application.ToDos.Parsers;

namespace Apollo.Application.Tests.ToDos.Parsers;

public class TimeOfDayAliasParserTests
{
  private readonly TimeOfDayAliasParser _parser = new();
  private readonly DateTime _ref = new(2025, 12, 30, 14, 30, 0, DateTimeKind.Utc);

  [Fact]
  public void TryParseWithTonightReturns8pm()
  {
    var result = _parser.TryParse("tonight", _ref);

    Assert.True(result.IsSuccess);
    Assert.Equal(new DateTime(2025, 12, 30, 20, 0, 0, DateTimeKind.Utc), result.Value);
    Assert.Equal(DateTimeKind.Utc, result.Value.Kind);
  }

  [Fact]
  public void TryParseWithThisMorningReturns9am()
  {
    var result = _parser.TryParse("this morning", _ref);

    Assert.True(result.IsSuccess);
    Assert.Equal(new DateTime(2025, 12, 30, 9, 0, 0, DateTimeKind.Utc), result.Value);
  }

  [Fact]
  public void TryParseWithThisAfternoonReturns2pm()
  {
    var result = _parser.TryParse("this afternoon", _ref);

    Assert.True(result.IsSuccess);
    Assert.Equal(new DateTime(2025, 12, 30, 14, 0, 0, DateTimeKind.Utc), result.Value);
  }

  [Fact]
  public void TryParseWithThisEveningReturns6pm()
  {
    var result = _parser.TryParse("this evening", _ref);

    Assert.True(result.IsSuccess);
    Assert.Equal(new DateTime(2025, 12, 30, 18, 0, 0, DateTimeKind.Utc), result.Value);
  }

  [Theory]
  [InlineData("TONIGHT")]
  [InlineData("  tonight  ")]
  [InlineData("THIS MORNING")]
  public void TryParseIsCaseInsensitive(string input)
  {
    var result = _parser.TryParse(input, _ref);

    Assert.True(result.IsSuccess);
  }

  [Theory]
  [InlineData("tomorrow")]
  [InlineData("in 10 minutes")]
  [InlineData("noon")]
  [InlineData("midnight")]
  [InlineData("next Monday")]
  public void TryParseWithNonAliasInputFails(string input)
  {
    var result = _parser.TryParse(input, _ref);

    Assert.True(result.IsFailed);
  }
}
