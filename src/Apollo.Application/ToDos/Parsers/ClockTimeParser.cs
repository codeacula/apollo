using System.Text.RegularExpressions;

using Apollo.Core.ToDos;

using FluentResults;

namespace Apollo.Application.ToDos.Parsers;

/// <summary>
/// Handles explicit clock-time expressions: "at 3pm", "at 15:00", "at noon", "at midnight".
/// Also handles standalone "noon" and "midnight".
/// </summary>
[TimeExpressionParser]
public sealed partial class ClockTimeParser : ITimeExpressionParser
{
  [GeneratedRegex(
    @"^\s*at\s+(?<time>\d{1,2}(?::\d{2})?\s*(?:am|pm)?|noon|midnight)\s*$",
    RegexOptions.IgnoreCase | RegexOptions.Compiled)]
  private static partial Regex AtTimePattern();

  [GeneratedRegex(@"^\s*(?<alias>noon|midnight)\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
  private static partial Regex StandaloneAliasPattern();

  public Result<DateTime> TryParse(string input, DateTime referenceTimeUtc)
  {
    var atMatch = AtTimePattern().Match(input);
    if (atMatch.Success)
    {
      return ResolveTimeAlias(atMatch.Groups["time"].Value.Trim().ToLowerInvariant(), referenceTimeUtc);
    }

    var standaloneMatch = StandaloneAliasPattern().Match(input);
    return standaloneMatch.Success switch
    {
      true => ResolveTimeAlias(standaloneMatch.Groups["alias"].Value.ToLowerInvariant(), referenceTimeUtc),
      false => Result.Fail<DateTime>($"'{input}' is not a clock-time expression")
    };
  }

  private static Result<DateTime> ResolveTimeAlias(string timeStr, DateTime reference)
  {

    return timeStr switch
    {
      "noon" => Result.Ok(TimeParserHelpers.EnsureUtc(reference.Date.AddHours(12))),
      "midnight" => Result.Ok(TimeParserHelpers.EnsureUtc(reference.Date.AddDays(1))),
      _ => TimeParserHelpers.ParseTimeOfDay(timeStr) switch
      {
        { IsSuccess: true } timeResult => Result.Ok(TimeParserHelpers.EnsureUtc(reference.Date.Add(timeResult.Value))),
        _ => Result.Fail<DateTime>($"Could not resolve clock time '{timeStr}'")
      }
    };
  }
}
