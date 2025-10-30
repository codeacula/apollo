using System.ComponentModel;

using Microsoft.SemanticKernel;

namespace Apollo.AI.Plugins;

#pragma warning disable RCS1102 // Make class static
public class TimePlugin
#pragma warning restore RCS1102 // Make class static
{
  [KernelFunction("get_time")]
  [Description("Gets the current time")]
  public static string GetTime()
  {
    return DateTime.Now.ToString("hh:mm tt", System.Globalization.CultureInfo.InvariantCulture);
  }

  [KernelFunction("get_date")]
  [Description("Gets the current date")]
  public static string GetDate()
  {
    return DateTime.Now.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
  }

  [KernelFunction("get_datetime")]
  [Description("Gets the current date and time")]
  public static string GetDateTime()
  {
    return DateTime.Now.ToString("yyyy-MM-dd hh:mm tt", System.Globalization.CultureInfo.InvariantCulture);
  }
}
