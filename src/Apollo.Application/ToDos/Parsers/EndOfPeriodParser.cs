using System.Text.RegularExpressions;

using Apollo.Core.ToDos;

using FluentResults;

namespace Apollo.Application.ToDos.Parsers;

/// <summary>
/// Handles end-of-period expressions: "end of day", "eod" (5 pm today),
/// and "end of week" (Friday 5 pm).
/// </summary>
[TimeExpressionParser]
public sealed partial class EndOfPeriodParser : ITimeExpressionParser
{
  [GeneratedRegex(@"^\s*(?:end\s+of\s+day|eod)\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
  private static partial Regex EndOfDayPattern();

  [GeneratedRegex(@"^\s*end\s+of\s+week\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
  private static partial Regex EndOfWeekPattern();

  public Result<DateTime> TryParse(string input, DateTime referenceTimeUtc)
  {
    if (EndOfDayPattern().IsMatch(input))
    {
      return Result.Ok(TimeParserHelpers.EnsureUtc(referenceTimeUtc.Date.AddHours(17)));
    }

    if (EndOfWeekPattern().IsMatch(input))
    {
      var daysUntilFriday = ((int)DayOfWeek.Friday - (int)referenceTimeUtc.DayOfWeek + 7) % 7;
      if (daysUntilFriday == 0)
      {
        daysUntilFriday = 7;
      }
      var endOfWeek = referenceTimeUtc.Date.AddDays(daysUntilFriday).AddHours(17);
      return Result.Ok(TimeParserHelpers.EnsureUtc(endOfWeek));
    }

    return Result.Fail<DateTime>($"'{input}' is not an end-of-period expression");
  }
}
