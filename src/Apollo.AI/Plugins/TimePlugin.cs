using System.ComponentModel;
using System.Globalization;

using Microsoft.SemanticKernel;

namespace Apollo.AI.Plugins;

public sealed class TimePlugin(TimeProvider timeProvider)
{
  private const string FullDateTimeFormat = "s";
  private const string TimeFormat = "T";

  [KernelFunction("get_datetime")]
  [Description("Gets the current date and time")]
  public string GetDateTime()
  {
    return timeProvider.GetUtcNow().ToString(FullDateTimeFormat, CultureInfo.InvariantCulture);
  }

  [KernelFunction("get_time")]
  [Description("Gets the current time")]
  public string GetTime()
  {
    return timeProvider.GetUtcNow().ToString(TimeFormat, CultureInfo.InvariantCulture);
  }
}
