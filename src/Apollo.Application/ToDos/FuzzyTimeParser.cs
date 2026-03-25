using System.Reflection;

using Apollo.Core.ToDos;

using FluentResults;

namespace Apollo.Application.ToDos;

/// <summary>
/// <para>
/// Orchestrates time expression parsing by delegating to a chain of
/// <see cref="ITimeExpressionParser"/> implementations that are discovered
/// automatically at startup via <see cref="TimeExpressionParserAttribute"/>.
/// </para>
/// <para>
/// To add a new time format, create a class that:
///   1. Implements <see cref="ITimeExpressionParser"/>
///   2. Is decorated with <see cref="TimeExpressionParserAttribute"/>
///   3. Has a public parameterless constructor
/// </para>
/// <para>No changes to this class or any registration code are required.</para>
/// </summary>
public sealed class FuzzyTimeParser : IFuzzyTimeParser
{
  private static readonly IReadOnlyList<ITimeExpressionParser> Parsers = DiscoverParsers();

  public Result<DateTime> TryParseFuzzyTime(string input, DateTime referenceTimeUtc, string? userTimeZoneId = null)
  {
    if (string.IsNullOrWhiteSpace(input))
    {
      return Result.Fail<DateTime>("Input is empty or whitespace");
    }

    var utcReference = referenceTimeUtc.Kind == DateTimeKind.Utc
      ? referenceTimeUtc
      : DateTime.SpecifyKind(referenceTimeUtc, DateTimeKind.Utc);

    var localReference = GetLocalReferenceTime(utcReference, userTimeZoneId);

    foreach (var parser in Parsers)
    {
      var reference = parser is Parsers.DurationParser ? utcReference : localReference;
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

    return [.. Assembly.GetExecutingAssembly()
      .GetTypes()
      .Where(t => t.IsClass
        && !t.IsAbstract
        && parserType.IsAssignableFrom(t)
        && t.IsDefined(attributeType, inherit: false))
      .Select(t => (ITimeExpressionParser)Activator.CreateInstance(t)!)];
  }

  private static DateTime GetLocalReferenceTime(DateTime utcReference, string? userTimeZoneId)
  {
    if (string.IsNullOrWhiteSpace(userTimeZoneId))
    {
      return utcReference;
    }

    try
    {
      var timeZone = TimeZoneInfo.FindSystemTimeZoneById(userTimeZoneId);
      var localReference = TimeZoneInfo.ConvertTimeFromUtc(utcReference, timeZone);
      return DateTime.SpecifyKind(localReference, DateTimeKind.Unspecified);
    }
    catch (TimeZoneNotFoundException)
    {
      return utcReference;
    }
    catch (InvalidTimeZoneException)
    {
      return utcReference;
    }
  }
}
