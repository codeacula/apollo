using NetCord;
using NetCord.Rest;

namespace Apollo.Discord.Components;

public sealed class DailyAlertSetupComponent : ComponentContainerProperties
{
  public const string ChannelSelectCustomId = "daily_alert_setup_channel";
  public const string RoleSelectCustomId = "daily_alert_setup_role";
  public const string ConfigureTimeButtonCustomId = "daily_alert_setup_time_button";
  public const string SaveButtonCustomId = "daily_alert_setup_save_button";
  public const int MaxMessageLength = 50;

  public DailyAlertSetupComponent(
      ulong? selectedChannelId = null,
      ulong? selectedRoleId = null,
      string? configuredTime = null,
      string? configuredMessage = null)
  {
    AccentColor = Constants.Colors.ApolloGreen;

    var components = new List<IComponentContainerComponentProperties>
        {
            new TextDisplayProperties("# Configure Daily Alerts"),
            new TextDisplayProperties("Set up your daily alert configuration. All fields are required before saving.")
        };

    var channelMenu = new ChannelMenuProperties(ChannelSelectCustomId)
    {
      ChannelTypes = [ChannelType.ForumGuildChannel],
      Placeholder = "Select a forum channel..."
    };

    components.Add(channelMenu);

    var roleMenu = new RoleMenuProperties(RoleSelectCustomId)
    {
      Placeholder = "Select a notification role..."
    };

    components.Add(roleMenu);

    string statusText = BuildStatusText(selectedChannelId, selectedRoleId, configuredTime, configuredMessage);
    components.Add(new TextDisplayProperties(statusText));

    var actionRow = new ActionRowProperties();
    var buttons = new List<IActionRowComponentProperties>
        {
            new ButtonProperties(ConfigureTimeButtonCustomId, "Configure Time & Message", ButtonStyle.Primary)
        };

    bool allConfigured = selectedChannelId.HasValue && selectedRoleId.HasValue &&
                       !string.IsNullOrWhiteSpace(configuredTime) &&
                       !string.IsNullOrWhiteSpace(configuredMessage);

    if (allConfigured)
    {
      buttons.Add(new ButtonProperties(SaveButtonCustomId, "Save Configuration", ButtonStyle.Success));
    }

    actionRow.Components = buttons;
    components.Add(actionRow);

    Components = components;
  }

  private static string BuildStatusText(ulong? channelId, ulong? roleId, string? time, string? message)
  {
    var lines = new List<string> { "**Current Configuration:**" };

    if (channelId.HasValue)
    {
      lines.Add($"✅ Forum Channel: <#{channelId.Value}>");
    }
    else
    {
      lines.Add("❌ Forum Channel: Not selected");
    }

    if (roleId.HasValue)
    {
      lines.Add($"✅ Notification Role: <@&{roleId.Value}>");
    }
    else
    {
      lines.Add("❌ Notification Role: Not selected");
    }

    if (!string.IsNullOrWhiteSpace(time))
    {
      lines.Add($"✅ Scheduled Time: {time}");
    }
    else
    {
      lines.Add("❌ Scheduled Time: Not configured");
    }

    if (!string.IsNullOrWhiteSpace(message))
    {
      string preview = message.Length > MaxMessageLength ? message[..MaxMessageLength] + "..." : message;
      lines.Add($"✅ Message: {preview}");
    }
    else
    {
      lines.Add("❌ Message: Not configured");
    }

    return string.Join("\n", lines);
  }
}
