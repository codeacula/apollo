using System;
using System.ComponentModel;
using System.Globalization;

using Microsoft.SemanticKernel;

namespace Apollo.AI.Plugins;

public class TimePlugin(TimeProvider timeProvider)
{
  [KernelFunction("convert_timezone")]
  [Description("Converts provided timestamp to the specified time zone")]
  public static string ConvertToTimeZone(string timestamp, string timeZoneId)
  {
    return !DateTimeOffset.TryParse(timestamp, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsedTimestamp)
      ? throw new ArgumentException("Invalid timestamp format.", nameof(timestamp))
      : TimeZoneInfo.ConvertTime(parsedTimestamp, TimeZoneInfo.FindSystemTimeZoneById(timeZoneId))
      .ToString("yyyy-MM-dd HH:mm:ss zzz", CultureInfo.InvariantCulture);
  }

  [KernelFunction("get_date")]
  [Description("Gets the current date")]
  public string GetDate()
  {
    return timeProvider.GetUtcNow().ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
  }

  [KernelFunction("get_datetime")]
  [Description("Gets the current date and time")]
  public string GetDateTime()
  {
    return timeProvider.GetUtcNow().ToString("yyyy-MM-dd hh:mm tt", CultureInfo.InvariantCulture);
  }

  [KernelFunction("get_time")]
  [Description("Gets the current time")]
  public string GetTime()
  {
    return timeProvider.GetUtcNow().ToString("hh:mm tt", CultureInfo.InvariantCulture);
  }
}
