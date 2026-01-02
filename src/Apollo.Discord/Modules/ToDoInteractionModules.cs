using Apollo.Discord.Components;

using NetCord.Services.ComponentInteractions;

namespace Apollo.Discord.Modules;

public class ToDoPriorityInteractionModule : ComponentInteractionModule<StringMenuInteractionContext>
{
  [ComponentInteraction(ToDoQuickCreateComponent.PrioritySelectCustomId)]
  public string HandlePriority() => $"Priority noted: {string.Join(", ", Context.SelectedValues)} (updates coming soon).";
}

public class ToDoEnergyInteractionModule : ComponentInteractionModule<StringMenuInteractionContext>
{
  [ComponentInteraction(ToDoQuickCreateComponent.EnergySelectCustomId)]
  public string HandleEnergy() => $"Energy level noted: {string.Join(", ", Context.SelectedValues)} (updates coming soon).";
}

public class ToDoInterestInteractionModule : ComponentInteractionModule<StringMenuInteractionContext>
{
  [ComponentInteraction(ToDoQuickCreateComponent.InterestSelectCustomId)]
  public string HandleInterest() => $"Interest noted: {string.Join(", ", Context.SelectedValues)} (updates coming soon).";
}

public class ToDoReminderInteractionModule : ComponentInteractionModule<ButtonInteractionContext>
{
  [ComponentInteraction(ToDoQuickCreateComponent.ReminderButtonCustomId)]
  public string HandleReminderButton() => "Reminder editing is coming soon. For now, reply with a time and I'll log it!";
}

public class ToDoEditInteractionModule : ComponentInteractionModule<ButtonInteractionContext>
{
  [ComponentInteraction("todo_edit_*")]
  public string HandleEditButton()
  {
    var toDoId = Context.Interaction.Data.CustomId.Replace("todo_edit_", string.Empty);
    return $"Edit interface for todo `{toDoId}` is coming soon!";
  }
}

public class ToDoDeleteInteractionModule : ComponentInteractionModule<ButtonInteractionContext>
{
  [ComponentInteraction("todo_delete_*")]
  public string HandleDeleteButton()
  {
    var toDoId = Context.Interaction.Data.CustomId.Replace("todo_delete_", string.Empty);
    return $"Delete confirmation for todo `{toDoId}` is coming soon!";
  }
}
