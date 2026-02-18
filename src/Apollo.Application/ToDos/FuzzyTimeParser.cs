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
/// - "in an hour", "in half an hour"
/// - "N minutes", "N hours" (without "in" prefix)
/// - "tomorrow", "tomorrow at 3pm"
/// - "next week", "next Monday"
/// - "at 3pm", "at 15:00", "at noon", "at midnight"
/// - "tonight", "this morning", "this afternoon", "this evening"
/// - "noon", "midnight"
/// - "on Tuesday", "on Friday at 3pm"
/// - "end of day", "eod", "end of week"
/// </summary>
public partial class FuzzyTimeParser : IFuzzyTimeParser
{
  // Pattern: "in N unit" where unit can be minutes, hours, days, weeks with various abbreviations
  [GeneratedRegex(
    @"^\s*in\s+(?<number>\d+)\s*(?<unit>minutes?|mins?|m|hours?|hrs?|h|days?|d|weeks?|w)\s*$",
    RegexOptions.IgnoreCase | RegexOptions.Compiled)]
  private static partial Regex InDurationPattern();

  // Pattern: "in an hour"
  [GeneratedRegex(@"^\s*in\s+an?\s+hour\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
  private static partial Regex InAnHourPattern();

  // Pattern: "in half an hour"
  [GeneratedRegex(@"^\s*in\s+half\s+an?\s+hour\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
  private static partial Regex InHalfAnHourPattern();

  // Pattern: "N unit" without "in" prefix (e.g., "5 minutes", "2 hours")
  [GeneratedRegex(
    @"^\s*(?<number>\d+)\s*(?<unit>minutes?|mins?|m|hours?|hrs?|h)\s*$",
    RegexOptions.IgnoreCase | RegexOptions.Compiled)]
  private static partial Regex BareNumberDurationPattern();

  // Pattern: "tomorrow at 3pm" / "tomorrow at 15:00"
  [GeneratedRegex(
    @"^\s*tomorrow\s+at\s+(?<time>\d{1,2}(?::\d{2})?\s*(?:am|pm)?)\s*$",
    RegexOptions.IgnoreCase | RegexOptions.Compiled)]
  private static partial Regex TomorrowAtTimePattern();

  // Pattern: "tomorrow"
  [GeneratedRegex(@"^\s*tomorrow\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
  private static partial Regex TomorrowPattern();

  // Pattern: "at 3pm" / "at 15:00" / "at noon" / "at midnight"
  [GeneratedRegex(
    @"^\s*at\s+(?<time>\d{1,2}(?::\d{2})?\s*(?:am|pm)?|noon|midnight)\s*$",
    RegexOptions.IgnoreCase | RegexOptions.Compiled)]
  private static partial Regex AtTimePattern();

  // Pattern: "tonight", "this morning", "this afternoon", "this evening"
  [GeneratedRegex(
    @"^\s*(?:(?<tonight>tonight)|this\s+(?<period>morning|afternoon|evening))\s*$",
    RegexOptions.IgnoreCase | RegexOptions.Compiled)]
  private static partial Regex TimeOfDayAliasPattern();

  // Pattern: standalone "noon" or "midnight"
  [GeneratedRegex(@"^\s*(?<alias>noon|midnight)\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
  private static partial Regex StandaloneTimeAliasPattern();

  // Pattern: "next Monday", "next Friday"
  [GeneratedRegex(
    @"^\s*next\s+(?<day>monday|tuesday|wednesday|thursday|friday|saturday|sunday)(?:\s+at\s+(?<time>\d{1,2}(?::\d{2})?\s*(?:am|pm)?))?\s*$",
    RegexOptions.IgnoreCase | RegexOptions.Compiled)]
  private static partial Regex NextDayOfWeekPattern();

  // Pattern: "next week"
  [GeneratedRegex(@"^\s*next\s+week\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
  private static partial Regex NextWeekPattern();

  // Pattern: "on Tuesday", "on Friday at 3pm"
  [GeneratedRegex(
    @"^\s*on\s+(?<day>monday|tuesday|wednesday|thursday|friday|saturday|sunday)(?:\s+at\s+(?<time>\d{1,2}(?::\d{2})?\s*(?:am|pm)?))?\s*$",
    RegexOptions.IgnoreCase | RegexOptions.Compiled)]
  private static partial Regex OnDayOfWeekPattern();

  // Pattern: "end of day", "eod"
  [GeneratedRegex(@"^\s*(?:end\s+of\s+day|eod)\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
  private static partial Regex EndOfDayPattern();

  // Pattern: "end of week"
  [GeneratedRegex(@"^\s*end\s+of\s+week\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
  private static partial Regex EndOfWeekPattern();

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

    // Try "in an hour" pattern
    if (InAnHourPattern().IsMatch(input))
    {
      return Result.Ok(DateTime.SpecifyKind(reference.AddHours(1), DateTimeKind.Utc));
    }

    // Try "in half an hour" pattern
    if (InHalfAnHourPattern().IsMatch(input))
    {
      return Result.Ok(DateTime.SpecifyKind(reference.AddMinutes(30), DateTimeKind.Utc));
    }

    // Try "N unit" without "in" prefix (e.g., "5 minutes", "2 hours")
    var bareDurationMatch = BareNumberDurationPattern().Match(input);
    if (bareDurationMatch.Success)
    {
      var number = int.Parse(bareDurationMatch.Groups["number"].Value, CultureInfo.InvariantCulture);
      var unit = bareDurationMatch.Groups["unit"].Value.ToLowerInvariant();

      var duration = unit switch
      {
        "m" or "min" or "mins" or "minute" or "minutes" => TimeSpan.FromMinutes(number),
        "h" or "hr" or "hrs" or "hour" or "hours" => TimeSpan.FromHours(number),
        _ => TimeSpan.Zero
      };

      if (duration > TimeSpan.Zero)
      {
        return Result.Ok(DateTime.SpecifyKind(reference.Add(duration), DateTimeKind.Utc));
      }
    }

    // Try "tomorrow at TIME" pattern
    var tomorrowAtMatch = TomorrowAtTimePattern().Match(input);
    if (tomorrowAtMatch.Success)
    {
      var timeResult = ParseTimeOfDay(tomorrowAtMatch.Groups["time"].Value);
      if (timeResult.IsSuccess)
      {
        var tomorrow = reference.Date.AddDays(1).Add(timeResult.Value);
        return Result.Ok(DateTime.SpecifyKind(tomorrow, DateTimeKind.Utc));
      }
    }

    // Try "tomorrow" pattern
    if (TomorrowPattern().IsMatch(input))
    {
      var tomorrow = reference.AddDays(1);
      return Result.Ok(DateTime.SpecifyKind(tomorrow, DateTimeKind.Utc));
    }

    // Try "at TIME" pattern (today)
    var atTimeMatch = AtTimePattern().Match(input);
    if (atTimeMatch.Success)
    {
      var timeStr = atTimeMatch.Groups["time"].Value.Trim().ToLowerInvariant();
      if (timeStr == "noon")
      {
        var noon = reference.Date.AddHours(12);
        return Result.Ok(DateTime.SpecifyKind(noon, DateTimeKind.Utc));
      }
      if (timeStr == "midnight")
      {
        var midnight = reference.Date.AddDays(1);
        return Result.Ok(DateTime.SpecifyKind(midnight, DateTimeKind.Utc));
      }

      var timeResult = ParseTimeOfDay(timeStr);
      if (timeResult.IsSuccess)
      {
        var today = reference.Date.Add(timeResult.Value);
        return Result.Ok(DateTime.SpecifyKind(today, DateTimeKind.Utc));
      }
    }

    // Try time-of-day aliases ("tonight", "this morning", etc.)
    var aliasMatch = TimeOfDayAliasPattern().Match(input);
    if (aliasMatch.Success)
    {
      if (aliasMatch.Groups["tonight"].Success)
      {
        var tonight = reference.Date.AddHours(20);
        return Result.Ok(DateTime.SpecifyKind(tonight, DateTimeKind.Utc));
      }

      var period = aliasMatch.Groups["period"].Value.ToLowerInvariant();
      var hours = period switch
      {
        "morning" => 9,
        "afternoon" => 14,
        "evening" => 18,
        _ => 12
      };

      var result = reference.Date.AddHours(hours);
      return Result.Ok(DateTime.SpecifyKind(result, DateTimeKind.Utc));
    }

    // Try standalone "noon" or "midnight"
    var standaloneMatch = StandaloneTimeAliasPattern().Match(input);
    if (standaloneMatch.Success)
    {
      var alias = standaloneMatch.Groups["alias"].Value.ToLowerInvariant();
      if (alias == "noon")
      {
        var noon = reference.Date.AddHours(12);
        return Result.Ok(DateTime.SpecifyKind(noon, DateTimeKind.Utc));
      }
      // midnight = start of next day
      var midnight = reference.Date.AddDays(1);
      return Result.Ok(DateTime.SpecifyKind(midnight, DateTimeKind.Utc));
    }

    // Try "next DAY" pattern (with optional time)
    var nextDayMatch = NextDayOfWeekPattern().Match(input);
    if (nextDayMatch.Success)
    {
      var targetDay = ParseDayOfWeek(nextDayMatch.Groups["day"].Value);
      var nextDate = GetNextDayOfWeek(reference, targetDay);

      if (nextDayMatch.Groups["time"].Success)
      {
        var timeResult = ParseTimeOfDay(nextDayMatch.Groups["time"].Value);
        if (timeResult.IsSuccess)
        {
          nextDate = nextDate.Date.Add(timeResult.Value);
        }
      }

      return Result.Ok(DateTime.SpecifyKind(nextDate, DateTimeKind.Utc));
    }

    // Try "next week" pattern
    if (NextWeekPattern().IsMatch(input))
    {
      var nextWeek = reference.AddDays(7);
      return Result.Ok(DateTime.SpecifyKind(nextWeek, DateTimeKind.Utc));
    }

    // Try "on DAY" pattern (with optional time)
    var onDayMatch = OnDayOfWeekPattern().Match(input);
    if (onDayMatch.Success)
    {
      var targetDay = ParseDayOfWeek(onDayMatch.Groups["day"].Value);
      var nextDate = GetNextDayOfWeek(reference, targetDay);

      if (onDayMatch.Groups["time"].Success)
      {
        var timeResult = ParseTimeOfDay(onDayMatch.Groups["time"].Value);
        if (timeResult.IsSuccess)
        {
          nextDate = nextDate.Date.Add(timeResult.Value);
        }
      }

      return Result.Ok(DateTime.SpecifyKind(nextDate, DateTimeKind.Utc));
    }

    // Try "end of day" / "eod" pattern
    if (EndOfDayPattern().IsMatch(input))
    {
      var endOfDay = reference.Date.AddHours(17);
      return Result.Ok(DateTime.SpecifyKind(endOfDay, DateTimeKind.Utc));
    }

    // Try "end of week" pattern (Friday 5pm)
    if (EndOfWeekPattern().IsMatch(input))
    {
      var daysUntilFriday = ((int)DayOfWeek.Friday - (int)reference.DayOfWeek + 7) % 7;
      if (daysUntilFriday == 0)
      {
        daysUntilFriday = 7;
      }
      var endOfWeek = reference.Date.AddDays(daysUntilFriday).AddHours(17);
      return Result.Ok(DateTime.SpecifyKind(endOfWeek, DateTimeKind.Utc));
    }

    return Result.Fail<DateTime>($"Could not parse '{input}' as a fuzzy time expression");
  }

  private static Result<TimeSpan> ParseTimeOfDay(string timeStr)
  {
    var normalized = timeStr.Trim().ToLowerInvariant();

    // Try parsing with AM/PM formats
    string[] formats = ["h:mm tt", "hh:mm tt", "htt", "hhtt", "h:mmtt", "hh:mmtt", "h tt", "hh tt"];
    if (DateTime.TryParseExact(normalized, formats, CultureInfo.InvariantCulture, DateTimeStyles.None,
      out var parsed))
    {
      return Result.Ok(parsed.TimeOfDay);
    }

    // Try 24-hour format (e.g., "15:00")
    if (DateTime.TryParseExact(normalized, ["HH:mm", "H:mm"], CultureInfo.InvariantCulture,
      DateTimeStyles.None, out var parsed24))
    {
      return Result.Ok(parsed24.TimeOfDay);
    }

    // Try bare hour (e.g., "15" as 15:00 — only for 24h values > 12)
    if (int.TryParse(normalized, CultureInfo.InvariantCulture, out var hour) && hour is >= 0 and <= 23)
    {
      return Result.Ok(TimeSpan.FromHours(hour));
    }

    return Result.Fail<TimeSpan>($"Could not parse '{timeStr}' as a time of day");
  }

  private static DayOfWeek ParseDayOfWeek(string dayStr)
  {
    return dayStr.Trim().ToLowerInvariant() switch
    {
      "monday" => DayOfWeek.Monday,
      "tuesday" => DayOfWeek.Tuesday,
      "wednesday" => DayOfWeek.Wednesday,
      "thursday" => DayOfWeek.Thursday,
      "friday" => DayOfWeek.Friday,
      "saturday" => DayOfWeek.Saturday,
      "sunday" => DayOfWeek.Sunday,
      _ => DayOfWeek.Monday
    };
  }

  private static DateTime GetNextDayOfWeek(DateTime reference, DayOfWeek target)
  {
    var daysUntil = ((int)target - (int)reference.DayOfWeek + 7) % 7;
    if (daysUntil == 0)
    {
      daysUntil = 7;
    }
    return reference.Date.AddDays(daysUntil);
  }
}
