using System.Text.RegularExpressions;

using Apollo.Core.ToDos;

using FluentResults;

namespace Apollo.Application.ToDos.Parsers;

/// <summary>
/// Handles named time-of-day aliases: "tonight", "this morning", "this afternoon", "this evening".
/// </summary>
[TimeExpressionParser]
public sealed partial class TimeOfDayAliasParser : ITimeExpressionParser
{
  [GeneratedRegex(
    @"^\s*(?:(?<tonight>tonight)|this\s+(?<period>morning|afternoon|evening))\s*$",
    RegexOptions.IgnoreCase | RegexOptions.Compiled)]
  private static partial Regex TimeOfDayAliasPattern();

  public Result<DateTime> TryParse(string input, DateTime referenceTimeUtc)
  {
    var match = TimeOfDayAliasPattern().Match(input);
    if (!match.Success)
    {
      return Result.Fail<DateTime>($"'{input}' is not a time-of-day alias");
    }

    if (match.Groups["tonight"].Success)
    {
      return Result.Ok(TimeParserHelpers.EnsureUtc(referenceTimeUtc.Date.AddHours(20)));
    }

    var hours = match.Groups["period"].Value.ToLowerInvariant() switch
    {
      "morning" => 9,
      "afternoon" => 14,
      "evening" => 18,
      _ => 12
    };

    return Result.Ok(TimeParserHelpers.EnsureUtc(referenceTimeUtc.Date.AddHours(hours)));
  }
}
