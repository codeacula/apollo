using NetCord;
using NetCord.Rest;

namespace Apollo.Discord.Components;

public partial class DailyAlertTimeConfigComponent : ComponentContainerProperties
{
    public const string ButtonCustomId = "daily_alert_time_config_button";

    public DailyAlertTimeConfigComponent() : base()
    {
        AccentColor = Constants.Colors.RydiaGreen;
        Components = [
            new TextDisplayProperties("# Configure Daily Update Schedule"),
            new TextDisplayProperties("Now let's set when and what the initial daily update post should say."),
            new ActionRowProperties()
            {
                Components = [
                    new ButtonProperties(ButtonCustomId, "Configure Time and Message", ButtonStyle.Primary)
                ]
            }
        ];
    }
}
