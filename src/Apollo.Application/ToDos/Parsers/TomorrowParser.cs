using System.Text.RegularExpressions;

using Apollo.Core.ToDos;

using FluentResults;

namespace Apollo.Application.ToDos.Parsers;

/// <summary>
/// Handles "tomorrow", "tomorrow at TIME", and "tomorrow morning/afternoon/evening" expressions.
/// Examples: "tomorrow", "tomorrow at 3pm", "tomorrow at 15:00", "tomorrow morning".
/// </summary>
[TimeExpressionParser]
public sealed partial class TomorrowParser : ITimeExpressionParser
{
  [GeneratedRegex(
    @"^\s*tomorrow\s+at\s+(?<time>\d{1,2}(?::\d{2})?\s*(?:am|pm)?)\s*$",
    RegexOptions.IgnoreCase | RegexOptions.Compiled)]
  private static partial Regex TomorrowAtTimePattern();

  [GeneratedRegex(
    @"^\s*tomorrow\s+(?<period>morning|afternoon|evening)\s*$",
    RegexOptions.IgnoreCase | RegexOptions.Compiled)]
  private static partial Regex TomorrowPeriodPattern();

  [GeneratedRegex(@"^\s*tomorrow\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
  private static partial Regex TomorrowPattern();

  public Result<DateTime> TryParse(string input, DateTime referenceTimeUtc)
  {
    var atMatch = TomorrowAtTimePattern().Match(input);
    if (atMatch.Success)
    {
      var timeResult = TimeParserHelpers.ParseTimeOfDay(atMatch.Groups["time"].Value);
      if (timeResult.IsSuccess)
      {
        var dt = referenceTimeUtc.Date.AddDays(1).Add(timeResult.Value);
        return Result.Ok(TimeParserHelpers.EnsureUtc(dt));
      }
    }

    var periodMatch = TomorrowPeriodPattern().Match(input);
    if (periodMatch.Success)
    {
      var hours = periodMatch.Groups["period"].Value.ToLowerInvariant() switch
      {
        "morning" => 9,
        "afternoon" => 14,
        "evening" => 18,
        _ => 9
      };
      var dt = referenceTimeUtc.Date.AddDays(1).AddHours(hours);
      return Result.Ok(TimeParserHelpers.EnsureUtc(dt));
    }

    return TomorrowPattern().IsMatch(input)
      ? Result.Ok(TimeParserHelpers.EnsureUtc(referenceTimeUtc.AddDays(1)))
      : Result.Fail<DateTime>($"'{input}' is not a tomorrow expression");
  }
}
