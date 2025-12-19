using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;

using Microsoft.SemanticKernel;

namespace Apollo.AI.Plugins;

public partial class TimePlugin(TimeProvider timeProvider)
{
  private static readonly Regex _fuzzyTimeRegex = FuzzyDatePatternRegex();

  private const string _dateFormat = "yyyy-MM-dd";
  private const string _fullDateTimeFormat = "s";
  private const string _timeFormat = "T";

  [KernelFunction("convert_timezone")]
  [Description("Converts provided timestamp to the specified time zone")]
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Needs to be an instance method for Semantic Kernel")]
  public string ConvertToTimeZone([Description("The timestamp to convert")] string timestamp, [Description("The target time zone ID")] string timeZoneId)
  {
    return !DateTimeOffset.TryParse(timestamp, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsedTimestamp)
      ? throw new ArgumentException("Invalid timestamp format.", nameof(timestamp))
      : TimeZoneInfo.ConvertTime(parsedTimestamp, TimeZoneInfo.FindSystemTimeZoneById(timeZoneId))
      .ToString(_fullDateTimeFormat, CultureInfo.InvariantCulture);
  }

  [KernelFunction("get_date")]
  [Description("Gets the current date")]
  public string GetDate()
  {
    return timeProvider.GetUtcNow().ToString(_dateFormat, CultureInfo.InvariantCulture);
  }

  [KernelFunction("get_datetime")]
  [Description("Gets the current date and time")]
  public string GetDateTime()
  {
    return timeProvider.GetUtcNow().ToString(_fullDateTimeFormat, CultureInfo.InvariantCulture);
  }

  [KernelFunction("get_fuzzy_date")]
  [Description("Gets a date and time relative to now from a natural language description, returning a UTC timestamp")]
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "Improves readability")]
  public string GetFuzzyDate(string description)
  {
    if (string.IsNullOrWhiteSpace(description))
    {
      throw new ArgumentException("Description cannot be empty.", nameof(description));
    }

    var normalizedDescription = description.Trim().ToLowerInvariant();

    if (TryParseRelativeTime(normalizedDescription, out var target))
    {
      return target.ToString(_fullDateTimeFormat, CultureInfo.InvariantCulture);
    }

    return DateTimeOffset.TryParse(normalizedDescription, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out target)
      ? target.ToUniversalTime().ToString(_fullDateTimeFormat, CultureInfo.InvariantCulture)
      : throw new ArgumentException("Unable to parse the provided description.", nameof(description));
  }

  [KernelFunction("get_time")]
  [Description("Gets the current time")]
  public string GetTime()
  {
    return timeProvider.GetUtcNow().ToString(_timeFormat, CultureInfo.InvariantCulture);
  }

  private bool TryParseRelativeTime(string description, out DateTimeOffset result)
  {
    var now = timeProvider.GetUtcNow().ToUniversalTime();

    if (description is "now" or "today")
    {
      result = now;
      return true;
    }

    if (description is "tomorrow")
    {
      result = now.AddDays(1);
      return true;
    }

    if (description is "yesterday")
    {
      result = now.AddDays(-1);
      return true;
    }

    if (description is "next week")
    {
      result = now.AddDays(7);
      return true;
    }

    if (description is "next month")
    {
      result = now.AddMonths(1);
      return true;
    }

    if (description is "next year")
    {
      result = now.AddYears(1);
      return true;
    }

    var match = _fuzzyTimeRegex.Match(description);
    if (!match.Success)
    {
      result = default;
      return false;
    }

    var quantity = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
    var unit = match.Groups[2].Value;

    result = unit switch
    {
      "second" or "seconds" => now.AddSeconds(quantity),
      "minute" or "minutes" => now.AddMinutes(quantity),
      "hour" or "hours" => now.AddHours(quantity),
      "day" or "days" => now.AddDays(quantity),
      "week" or "weeks" => now.AddDays((double)quantity * 7),
      "month" or "months" => now.AddMonths(quantity),
      "year" or "years" => now.AddYears(quantity),
      _ => now
    };

    return true;
  }

  [GeneratedRegex(@"^(?:in\s+)?(\d+)\s+(second|seconds|minute|minutes|hour|hours|day|days|week|weeks|month|months|year|years)(?:\s+from\s+now)?$")]
  private static partial Regex FuzzyDatePatternRegex();
}
