using Apollo.Application.ToDos.Parsers;
using Apollo.Core.ToDos;

namespace Apollo.Application.Tests.ToDos.Parsers;

/// <summary>
/// Verifies that <see cref="TimeExpressionParserAttribute"/> auto-discovery
/// works correctly and that all expected parser types are registered.
/// </summary>
public class TimeExpressionParserDiscoveryTests
{
  private static readonly IReadOnlyList<Type> ExpectedParsers =
  [
    typeof(DurationParser),
    typeof(TomorrowParser),
    typeof(ClockTimeParser),
    typeof(TimeOfDayAliasParser),
    typeof(DayOfWeekParser),
    typeof(EndOfPeriodParser),
  ];

  [Fact]
  public void AllExpectedParserTypesAreDecoratedWithAttribute()
  {
    foreach (var type in ExpectedParsers)
    {
      Assert.True(
        type.IsDefined(typeof(TimeExpressionParserAttribute), inherit: false),
        $"{type.Name} is missing [TimeExpressionParser]");
    }
  }

  [Fact]
  public void AllExpectedParserTypesImplementITimeExpressionParser()
  {
    foreach (var type in ExpectedParsers)
    {
      Assert.True(
        typeof(ITimeExpressionParser).IsAssignableFrom(type),
        $"{type.Name} does not implement ITimeExpressionParser");
    }
  }

  [Fact]
  public void AllExpectedParserTypesHavePublicParameterlessConstructor()
  {
    foreach (var type in ExpectedParsers)
    {
      var ctor = type.GetConstructor(Type.EmptyTypes);
      Assert.NotNull(ctor);
    }
  }
}
