using NetCord.Rest;

namespace Apollo.Discord.Components;

public partial class DailyAlertTimeConfigModal : ModalProperties
{
    public new const string CustomId = "daily_alert_time_config_modal";
    public const string TimeInputCustomId = "daily_alert_time_input";
    public const string MessageInputCustomId = "daily_alert_message_input";
    public const string DefaultTime = "06:00";
    public const string DefaultMessage = "Good morning! What are your goals for today?";

    public DailyAlertTimeConfigModal() : base(CustomId, "Configure Daily Update")
    {
        Components = [
            new LabelProperties("Time (HH:mm format, 24-hour)", new TextInputProperties(TimeInputCustomId, TextInputStyle.Short)
            {
                Placeholder = DefaultTime,
                Value = DefaultTime,
                Required = false,
                MinLength = 5,
                MaxLength = 5
            }),
            new LabelProperties("Initial Message Text", new TextInputProperties(MessageInputCustomId, TextInputStyle.Paragraph)
            {
                Placeholder = DefaultMessage,
                Value = DefaultMessage,
                Required = false,
                MinLength = 1,
                MaxLength = 2000
            })
        ];
    }
}
