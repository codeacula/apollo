using Apollo.Domain.ToDos.Models;

using NetCord;
using NetCord.Rest;

namespace Apollo.Discord.Components;

public class ToDoQuickCreateComponent : ComponentContainerProperties
{
  public const string PrioritySelectCustomId = "todo_priority_select";
  public const string EnergySelectCustomId = "todo_energy_select";
  public const string InterestSelectCustomId = "todo_interest_select";
  public const string ReminderButtonCustomId = "todo_reminder_button";

  public ToDoQuickCreateComponent(ToDo todo, DateTime? reminderDate)
  {
    AccentColor = Constants.Colors.ApolloGreen;

    List<IComponentContainerComponentProperties> components =
    [
      new TextDisplayProperties("# To-Do Created"),
      new TextDisplayProperties($"**{todo.Description.Value}**"),
      new TextDisplayProperties($"ID: `{todo.Id.Value}`")
    ];

    if (reminderDate.HasValue)
    {
      var unix = new DateTimeOffset(reminderDate.Value).ToUnixTimeSeconds();
      components.Add(new TextDisplayProperties($"🔔 Reminder set for <t:{unix}:F>"));
    }
    else
    {
      components.Add(new TextDisplayProperties("🔔 No reminder set. Add one with the button below."));
    }

    components.Add(new TextDisplayProperties("### Priority"));
    components.Add(new StringMenuProperties(PrioritySelectCustomId)
    {
      new("Blue (Default)", "blue") { Emoji = EmojiProperties.Standard("🔵"), Description = "Baseline priority", Default = true },
      new("Green", "green") { Emoji = EmojiProperties.Standard("🟢"), Description = "Low urgency" },
      new("Yellow", "yellow") { Emoji = EmojiProperties.Standard("🟡"), Description = "Medium urgency" },
      new("Red", "red") { Emoji = EmojiProperties.Standard("🔴"), Description = "High urgency" },
    });

    components.Add(new TextDisplayProperties("### Energy"));
    components.Add(new StringMenuProperties(EnergySelectCustomId)
    {
      new("Blue", "blue") { Emoji = EmojiProperties.Standard("🔵"), Description = "Default energy", Default = true },
      new("Green", "green") { Emoji = EmojiProperties.Standard("🟢"), Description = "Low energy" },
      new("Yellow", "yellow") { Emoji = EmojiProperties.Standard("🟡"), Description = "Medium energy" },
      new("Red", "red") { Emoji = EmojiProperties.Standard("🔴"), Description = "High energy" },
    });

    components.Add(new TextDisplayProperties("### Interest"));
    components.Add(new StringMenuProperties(InterestSelectCustomId)
    {
      new("Blue", "blue") { Emoji = EmojiProperties.Standard("🔵"), Description = "Default interest", Default = true },
      new("Green", "green") { Emoji = EmojiProperties.Standard("🟢"), Description = "Low interest" },
      new("Yellow", "yellow") { Emoji = EmojiProperties.Standard("🟡"), Description = "Medium interest" },
      new("Red", "red") { Emoji = EmojiProperties.Standard("🔴"), Description = "High interest" },
    });

    components.Add(new ActionRowProperties
    {
      Components =
      [
        new ButtonProperties(ReminderButtonCustomId, "Add / Update Reminder", ButtonStyle.Primary),
      ]
    });

    Components = components;
  }
}
