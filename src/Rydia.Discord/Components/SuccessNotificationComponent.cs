using NetCord.Rest;

namespace Rydia.Discord.Components;

public partial class SuccessNotificationComponent : ComponentContainerProperties
{
    public SuccessNotificationComponent(string heading, string message) : base()
    {
        AccentColor = Constants.Colors.Success;
        Components =
        [
            new TextDisplayProperties($"# {heading}"),
            new TextDisplayProperties(message)
        ];
    }
}
