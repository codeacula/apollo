using Apollo.Application.ToDos;

namespace Apollo.Application.Tests.ToDos;

public class FuzzyTimeParserTests
{
  private readonly FuzzyTimeParser _parser = new();
  private readonly DateTime _referenceTime = new(2025, 12, 30, 14, 30, 0, DateTimeKind.Utc);

  [Theory]
  [InlineData("in half an hour", 2025, 12, 30, 15, 0)]
  [InlineData("tomorrow at 3pm", 2025, 12, 31, 15, 0)]
  [InlineData("at midnight", 2025, 12, 31, 0, 0)]
  [InlineData("tonight", 2025, 12, 30, 20, 0)]
  [InlineData("next Tuesday", 2026, 1, 6, 0, 0)]
  [InlineData("end of day", 2025, 12, 30, 17, 0)]
  public void TryParseFuzzyTimeDelegatesToDiscoveredParsers(
    string input,
    int year,
    int month,
    int day,
    int hour,
    int minute)
  {
    var result = _parser.TryParseFuzzyTime(input, _referenceTime);

    Assert.True(result.IsSuccess);
    Assert.Equal(new DateTime(year, month, day, hour, minute, 0, DateTimeKind.Utc), result.Value);
    Assert.Equal(DateTimeKind.Utc, result.Value.Kind);
  }

  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  [InlineData(null)]
  public void TryParseFuzzyTimeFailsForEmptyInput(string? input)
  {
    var result = _parser.TryParseFuzzyTime(input!, _referenceTime);

    Assert.True(result.IsFailed);
    Assert.Contains("empty", result.Errors[0].Message, StringComparison.OrdinalIgnoreCase);
  }

  [Theory]
  [InlineData("2025-12-31T10:00:00")]
  [InlineData("hello world")]
  [InlineData("yesterday")]
  public void TryParseFuzzyTimeFailsForUnknownInput(string input)
  {
    var result = _parser.TryParseFuzzyTime(input, _referenceTime);

    Assert.True(result.IsFailed);
    Assert.Contains("Could not parse", result.Errors[0].Message);
  }

  [Fact]
  public void TryParseFuzzyTimePreservesUtcKindFromReference()
  {
    var result = _parser.TryParseFuzzyTime("in 10 minutes", _referenceTime);

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
