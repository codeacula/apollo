using System.Globalization;
using System.Text.RegularExpressions;

using FluentResults;

namespace Apollo.Application.ToDos.Parsers;

/// <summary>
/// Shared helpers used by multiple <see cref="Apollo.Core.ToDos.ITimeExpressionParser"/> implementations.
/// </summary>
internal static partial class TimeParserHelpers
{
  [GeneratedRegex(
    @"(?<hour>\d{1,2})(?::(?<minute>\d{2}))?\s*(?<ampm>am|pm)?",
    RegexOptions.IgnoreCase | RegexOptions.Compiled)]
  private static partial Regex TimeComponentPattern();

  internal static Result<TimeSpan> ParseTimeOfDay(string timeStr)
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

    // Try bare hour (e.g., "15" as 15:00 — only for 24h values >= 0)
    if (int.TryParse(normalized, CultureInfo.InvariantCulture, out var hour) && hour is >= 0 and <= 23)
    {
      return Result.Ok(TimeSpan.FromHours(hour));
    }

    return Result.Fail<TimeSpan>($"Could not parse '{timeStr}' as a time of day");
  }

  internal static DayOfWeek ParseDayOfWeek(string dayStr)
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

  internal static DateTime GetNextDayOfWeek(DateTime reference, DayOfWeek target)
  {
    var daysUntil = ((int)target - (int)reference.DayOfWeek + 7) % 7;
    if (daysUntil == 0)
    {
      daysUntil = 7;
    }
    return reference.Date.AddDays(daysUntil);
  }

  internal static DateTime EnsureUtc(DateTime dt) =>
    dt.Kind == DateTimeKind.Utc ? dt : DateTime.SpecifyKind(dt, DateTimeKind.Utc);
}
