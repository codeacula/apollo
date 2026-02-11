using Apollo.Domain.ToDos.Models;

using NetCord;
using NetCord.Rest;

namespace Apollo.Discord.Components;

public class ToDoQuickCreateComponent : ComponentContainerProperties
{
  public const string PrioritySelectPrefix = "todo_priority_select";
  public const string EnergySelectPrefix = "todo_energy_select";
  public const string InterestSelectPrefix = "todo_interest_select";
  public const string ReminderButtonPrefix = "todo_reminder_button";

  public const string PrioritySelectCustomId = PrioritySelectPrefix;
  public const string EnergySelectCustomId = EnergySelectPrefix;
  public const string InterestSelectCustomId = InterestSelectPrefix;
  public const string ReminderButtonCustomId = ReminderButtonPrefix;

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
    components.Add(new StringMenuProperties($"{PrioritySelectPrefix}:{todo.Id.Value}")
    {
      new("Blue (Default)", "blue") { Emoji = EmojiProperties.Standard("🔵"), Description = "Baseline priority", Default = true },
      new("Green", "green") { Emoji = EmojiProperties.Standard("🟢"), Description = "Low urgency" },
      new("Yellow", "yellow") { Emoji = EmojiProperties.Standard("🟡"), Description = "Medium urgency" },
      new("Red", "red") { Emoji = EmojiProperties.Standard("🔴"), Description = "High urgency" },
    });

    components.Add(new TextDisplayProperties("### Energy"));
    components.Add(new StringMenuProperties($"{EnergySelectPrefix}:{todo.Id.Value}")
    {
      new("Blue", "blue") { Emoji = EmojiProperties.Standard("🔵"), Description = "Minimal energy" },
      new("Green (Default)", "green") { Emoji = EmojiProperties.Standard("🟢"), Description = "Normal energy", Default = true },
      new("Yellow", "yellow") { Emoji = EmojiProperties.Standard("🟡"), Description = "Medium energy" },
      new("Red", "red") { Emoji = EmojiProperties.Standard("🔴"), Description = "High energy" },
    });

    components.Add(new TextDisplayProperties("### Interest"));
    components.Add(new StringMenuProperties($"{InterestSelectPrefix}:{todo.Id.Value}")
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
        new ButtonProperties($"{ReminderButtonPrefix}:{todo.Id.Value}", "Add / Update Reminder", ButtonStyle.Primary),
      ]
    });

    Components = components;
  }
}
