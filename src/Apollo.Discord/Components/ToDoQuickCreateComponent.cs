using Apollo.Domain.ToDos.Models;

using NetCord;

namespace Apollo.Discord.Components;

/// <summary>
/// Component builder for quick ToDo creation feedback with priority, energy, and interest selection.
/// </summary>
public sealed class ToDoQuickCreateComponent
{
  public const string PrioritySelectCustomId = "todo_priority_select";
  public const string EnergySelectCustomId = "todo_energy_select";
  public const string InterestSelectCustomId = "todo_interest_select";
  public const string ReminderButtonCustomId = "todo_reminder_button";

  private readonly ComponentContainerProperties _container;

  public ToDoQuickCreateComponent(ToDo toDo, DateTime? reminderDate)
  {
    _container = new ComponentContainerProperties
    {
      AccentColor = new Color(0x10B981),
      Components =
      [
        new TextDisplayProperties($"✅ **To-Do Created**: {toDo.Description.Value}"),
      ]
    };
  }

  public static implicit operator ComponentContainerProperties(ToDoQuickCreateComponent component) => component._container;
  public static implicit operator ToDoQuickCreateComponent(ComponentContainerProperties container) => new(container);

  private ToDoQuickCreateComponent(ComponentContainerProperties container)
  {
    _container = container;
  }
}
