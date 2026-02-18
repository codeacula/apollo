using System.Text.RegularExpressions;

using Apollo.Core.ToDos;

using FluentResults;

namespace Apollo.Application.ToDos.Parsers;

/// <summary>
/// Handles day-of-week references: "next Monday", "on Friday", "on Friday at 3pm",
/// "next Monday at 9am", and "next week".
/// </summary>
[TimeExpressionParser]
public sealed partial class DayOfWeekParser : ITimeExpressionParser
{
  private const string DayGroup =
    "monday|tuesday|wednesday|thursday|friday|saturday|sunday";

  private const string OptionalTime =
    @"(?:\s+at\s+(?<time>\d{1,2}(?::\d{2})?\s*(?:am|pm)?))?";

  [GeneratedRegex(
    $@"^\s*next\s+(?<day>{DayGroup}){OptionalTime}\s*$",
    RegexOptions.IgnoreCase | RegexOptions.Compiled)]
  private static partial Regex NextDayPattern();

  [GeneratedRegex(
    $@"^\s*on\s+(?<day>{DayGroup}){OptionalTime}\s*$",
    RegexOptions.IgnoreCase | RegexOptions.Compiled)]
  private static partial Regex OnDayPattern();

  [GeneratedRegex(@"^\s*next\s+week\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
  private static partial Regex NextWeekPattern();

  public Result<DateTime> TryParse(string input, DateTime referenceTimeUtc)
  {
    if (NextWeekPattern().IsMatch(input))
    {
      return Result.Ok(TimeParserHelpers.EnsureUtc(referenceTimeUtc.AddDays(7)));
    }

    var nextMatch = NextDayPattern().Match(input);
    if (nextMatch.Success)
    {
      return ResolveDayMatch(nextMatch, referenceTimeUtc);
    }

    var onMatch = OnDayPattern().Match(input);
    if (onMatch.Success)
    {
      return ResolveDayMatch(onMatch, referenceTimeUtc);
    }

    return Result.Fail<DateTime>($"'{input}' is not a day-of-week expression");
  }

  private static Result<DateTime> ResolveDayMatch(Match match, DateTime reference)
  {
    var target = TimeParserHelpers.ParseDayOfWeek(match.Groups["day"].Value);
    var date = TimeParserHelpers.GetNextDayOfWeek(reference, target);

    if (match.Groups["time"].Success)
    {
      var timeResult = TimeParserHelpers.ParseTimeOfDay(match.Groups["time"].Value);
      if (timeResult.IsSuccess)
      {
        date = date.Date.Add(timeResult.Value);
      }
    }

    return Result.Ok(TimeParserHelpers.EnsureUtc(date));
  }
}
