using NetCord.Rest;

namespace Apollo.Discord.Components;

public class SuccessNotificationComponent : ComponentContainerProperties
{
  public SuccessNotificationComponent(string heading, string message)
  {
    AccentColor = Constants.Colors.Success;
    Components =
    [
        new TextDisplayProperties($"# {heading}"),
            new TextDisplayProperties(message)
    ];
  }
}
