using NetCord.Rest;

namespace Apollo.Discord.Components;

public partial class DailyAlertTimeConfigModal : ModalProperties
{
    public new const string CustomId = "daily_alert_time_config_modal";
    public const string TimeInputCustomId = "daily_alert_time_input";
    public const string MessageInputCustomId = "daily_alert_message_input";

    public DailyAlertTimeConfigModal() : base(CustomId, "Configure Daily Update")
    {
        Components = [
            new LabelProperties("Time (HH:mm format, 24-hour)", new TextInputProperties(TimeInputCustomId, TextInputStyle.Short)
            {
                Placeholder = "06:00",
                Value = "06:00",
                Required = true,
                MinLength = 5,
                MaxLength = 5
            }),
            new LabelProperties("Initial Message Text", new TextInputProperties(MessageInputCustomId, TextInputStyle.Paragraph)
            {
                Placeholder = "Good morning! What are your goals for today?",
                Value = "Good morning! What are your goals for today?",
                Required = true,
                MinLength = 1,
                MaxLength = 2000
            })
        ];
    }
}
