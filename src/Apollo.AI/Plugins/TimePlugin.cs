using System.ComponentModel;

using Microsoft.SemanticKernel;

namespace Apollo.AI.Plugins;

public class TimePlugin(TimeProvider timeProvider)
{
  [KernelFunction("get_time")]
  [Description("Gets the current time")]
  public string GetTime()
  {
    var currentTime = timeProvider.GetUtcNow();
    return currentTime.ToString("hh:mm tt", System.Globalization.CultureInfo.InvariantCulture);
  }

  [KernelFunction("get_date")]
  [Description("Gets the current date")]
  public string GetDate()
  {
    var currentTime = timeProvider.GetUtcNow();
    return currentTime.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
  }

  [KernelFunction("get_datetime")]
  [Description("Gets the current date and time")]
  public string GetDateTime()
  {
    var currentTime = timeProvider.GetUtcNow();
    return currentTime.ToString("yyyy-MM-dd hh:mm tt", System.Globalization.CultureInfo.InvariantCulture);
  }
}
