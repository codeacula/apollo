using System.Reflection;

using Apollo.Core.ToDos;

using FluentResults;

namespace Apollo.Application.ToDos;

/// <summary>
/// Orchestrates time expression parsing by delegating to a chain of
/// <see cref="ITimeExpressionParser"/> implementations that are discovered
/// automatically at startup via <see cref="TimeExpressionParserAttribute"/>.
///
/// To add a new time format, create a class that:
///   1. Implements <see cref="ITimeExpressionParser"/>
///   2. Is decorated with <see cref="TimeExpressionParserAttribute"/>
///   3. Has a public parameterless constructor
///
/// No changes to this class or any registration code are required.
/// </summary>
public sealed class FuzzyTimeParser : IFuzzyTimeParser
{
  private static readonly IReadOnlyList<ITimeExpressionParser> Parsers = DiscoverParsers();

  public Result<DateTime> TryParseFuzzyTime(string input, DateTime referenceTimeUtc)
  {
    if (string.IsNullOrWhiteSpace(input))
    {
      return Result.Fail<DateTime>("Input is empty or whitespace");
    }

    var reference = referenceTimeUtc.Kind == DateTimeKind.Utc
      ? referenceTimeUtc
      : DateTime.SpecifyKind(referenceTimeUtc, DateTimeKind.Utc);

    foreach (var parser in Parsers)
    {
      var result = parser.TryParse(input, reference);
      if (result.IsSuccess)
      {
        return result;
      }
    }

    return Result.Fail<DateTime>($"Could not parse '{input}' as a fuzzy time expression");
  }

  private static List<ITimeExpressionParser> DiscoverParsers()
  {
    var parserType = typeof(ITimeExpressionParser);
    var attributeType = typeof(TimeExpressionParserAttribute);

    return Assembly.GetExecutingAssembly()
      .GetTypes()
      .Where(t => t.IsClass
        && !t.IsAbstract
        && parserType.IsAssignableFrom(t)
        && t.IsDefined(attributeType, inherit: false))
      .Select(t => (ITimeExpressionParser)Activator.CreateInstance(t)!)
      .ToList();
  }
}
