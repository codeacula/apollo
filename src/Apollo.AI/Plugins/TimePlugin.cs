using System.ComponentModel;
using System.Globalization;

using Microsoft.SemanticKernel;

namespace Apollo.AI.Plugins;

public class TimePlugin(TimeProvider timeProvider)
{
  private const string _fullDateTimeFormat = "s";
  private const string _timeFormat = "T";

  [KernelFunction("get_datetime")]
  [Description("Gets the current date and time")]
  public string GetDateTime()
  {
    return timeProvider.GetUtcNow().ToString(_fullDateTimeFormat, CultureInfo.InvariantCulture);
  }

  [KernelFunction("get_time")]
  [Description("Gets the current time")]
  public string GetTime()
  {
    return timeProvider.GetUtcNow().ToString(_timeFormat, CultureInfo.InvariantCulture);
  }
}
