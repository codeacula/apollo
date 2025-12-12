using System.ComponentModel;

using Microsoft.SemanticKernel;

namespace Apollo.AI.Plugins;

public class TimePlugin(TimeProvider timeProvider)
{
  [KernelFunction("get_time")]
  [Description("Gets the current time")]
  public string GetTime()
  {
    return timeProvider.GetUtcNow().ToString("hh:mm tt", System.Globalization.CultureInfo.InvariantCulture);
  }

  [KernelFunction("get_date")]
  [Description("Gets the current date")]
  public string GetDate()
  {
    return timeProvider.GetUtcNow().ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
  }

  [KernelFunction("get_datetime")]
  [Description("Gets the current date and time")]
  public string GetDateTime()
  {
    return timeProvider.GetUtcNow().ToString("yyyy-MM-dd hh:mm tt", System.Globalization.CultureInfo.InvariantCulture);
  }
}
