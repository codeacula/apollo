using System.Globalization;
using System.Text.RegularExpressions;

using Apollo.Core.ToDos;

using FluentResults;

namespace Apollo.Application.ToDos.Parsers;

/// <summary>
/// Handles "in N unit" and bare "N unit" duration expressions.
/// Examples: "in 10 minutes", "in 2 hours", "in 3 days", "in 1 week",
///           "5 minutes", "2 hours".
/// </summary>
[TimeExpressionParser]
public sealed partial class DurationParser : ITimeExpressionParser
{
  // "in N unit" — minutes, hours, days, weeks with common abbreviations
  [GeneratedRegex(
    @"^\s*in\s+(?<number>\d+)\s*(?<unit>minutes?|mins?|m|hours?|hrs?|h|days?|d|weeks?|w)\s*$",
    RegexOptions.IgnoreCase | RegexOptions.Compiled)]
  private static partial Regex InDurationPattern();

  // "N unit" without the "in" prefix — minutes and hours only
  [GeneratedRegex(
    @"^\s*(?<number>\d+)\s*(?<unit>minutes?|mins?|m|hours?|hrs?|h)\s*$",
    RegexOptions.IgnoreCase | RegexOptions.Compiled)]
  private static partial Regex BareDurationPattern();

  // "in an hour" / "in a hour"
  [GeneratedRegex(@"^\s*in\s+an?\s+hour\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
  private static partial Regex InAnHourPattern();

  // "in half an hour"
  [GeneratedRegex(@"^\s*in\s+half\s+an?\s+hour\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
  private static partial Regex InHalfAnHourPattern();

  public Result<DateTime> TryParse(string input, DateTime referenceTimeUtc)
  {
    if (InAnHourPattern().IsMatch(input))
    {
      return Result.Ok(TimeParserHelpers.EnsureUtc(referenceTimeUtc.AddHours(1)));
    }

    if (InHalfAnHourPattern().IsMatch(input))
    {
      return Result.Ok(TimeParserHelpers.EnsureUtc(referenceTimeUtc.AddMinutes(30)));
    }

    var inMatch = InDurationPattern().Match(input);
    if (inMatch.Success)
    {
      var duration = ToDuration(inMatch);
      if (duration > TimeSpan.Zero)
      {
        return Result.Ok(TimeParserHelpers.EnsureUtc(referenceTimeUtc.Add(duration)));
      }
    }

    var bareMatch = BareDurationPattern().Match(input);
    if (bareMatch.Success)
    {
      var duration = ToDuration(bareMatch);
      if (duration > TimeSpan.Zero)
      {
        return Result.Ok(TimeParserHelpers.EnsureUtc(referenceTimeUtc.Add(duration)));
      }
    }

    return Result.Fail<DateTime>($"'{input}' is not a duration expression");
  }

  private static TimeSpan ToDuration(Match match)
  {
    var number = int.Parse(match.Groups["number"].Value, CultureInfo.InvariantCulture);
    return match.Groups["unit"].Value.ToLowerInvariant() switch
    {
      "m" or "min" or "mins" or "minute" or "minutes" => TimeSpan.FromMinutes(number),
      "h" or "hr" or "hrs" or "hour" or "hours" => TimeSpan.FromHours(number),
      "d" or "day" or "days" => TimeSpan.FromDays(number),
      "w" or "week" or "weeks" => TimeSpan.FromDays(number * 7),
      _ => TimeSpan.Zero
    };
  }
}
