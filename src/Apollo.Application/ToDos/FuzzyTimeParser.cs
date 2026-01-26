using System.Globalization;
using System.Text.RegularExpressions;

using Apollo.Core.ToDos;

using FluentResults;

namespace Apollo.Application.ToDos;

/// <summary>
/// Parses fuzzy/relative time expressions into absolute DateTime values.
/// Supports patterns like:
/// - "in N minute(s)", "in N min(s)", "in Nm"
/// - "in N hour(s)", "in N hr(s)", "in Nh"
/// - "in N day(s)", "in Nd"
/// - "in N week(s)", "in Nw"
/// - "tomorrow"
/// - "next week"
/// </summary>
public partial class FuzzyTimeParser : IFuzzyTimeParser
{
  // Pattern: "in N unit" where unit can be minutes, hours, days, weeks with various abbreviations
  [GeneratedRegex(
    @"^\s*in\s+(?<number>\d+)\s*(?<unit>minutes?|mins?|m|hours?|hrs?|h|days?|d|weeks?|w)\s*$",
    RegexOptions.IgnoreCase | RegexOptions.Compiled)]
  private static partial Regex InDurationPattern();

  // Pattern: "tomorrow"
  [GeneratedRegex(@"^\s*tomorrow\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
  private static partial Regex TomorrowPattern();

  // Pattern: "next week"
  [GeneratedRegex(@"^\s*next\s+week\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
  private static partial Regex NextWeekPattern();

  public Result<DateTime> TryParseFuzzyTime(string input, DateTime referenceTimeUtc)
  {
    if (string.IsNullOrWhiteSpace(input))
    {
      return Result.Fail<DateTime>("Input is empty or whitespace");
    }

    // Ensure reference time is UTC
    var reference = referenceTimeUtc.Kind == DateTimeKind.Utc
      ? referenceTimeUtc
      : DateTime.SpecifyKind(referenceTimeUtc, DateTimeKind.Utc);

    // Try "in N unit" pattern
    var durationMatch = InDurationPattern().Match(input);
    if (durationMatch.Success)
    {
      var number = int.Parse(durationMatch.Groups["number"].Value, CultureInfo.InvariantCulture);
      var unit = durationMatch.Groups["unit"].Value.ToLowerInvariant();

      var duration = unit switch
      {
        "m" or "min" or "mins" or "minute" or "minutes" => TimeSpan.FromMinutes(number),
        "h" or "hr" or "hrs" or "hour" or "hours" => TimeSpan.FromHours(number),
        "d" or "day" or "days" => TimeSpan.FromDays(number),
        "w" or "week" or "weeks" => TimeSpan.FromDays(number * 7),
        _ => TimeSpan.Zero
      };

      if (duration > TimeSpan.Zero)
      {
        return Result.Ok(DateTime.SpecifyKind(reference.Add(duration), DateTimeKind.Utc));
      }
    }

    // Try "tomorrow" pattern
    if (TomorrowPattern().IsMatch(input))
    {
      var tomorrow = reference.AddDays(1);
      return Result.Ok(DateTime.SpecifyKind(tomorrow, DateTimeKind.Utc));
    }

    // Try "next week" pattern
    if (NextWeekPattern().IsMatch(input))
    {
      var nextWeek = reference.AddDays(7);
      return Result.Ok(DateTime.SpecifyKind(nextWeek, DateTimeKind.Utc));
    }

    return Result.Fail<DateTime>($"Could not parse '{input}' as a fuzzy time expression");
  }
}
