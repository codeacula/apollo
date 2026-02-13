using Apollo.Discord.Components;

using NetCord.Services.ComponentInteractions;

namespace Apollo.Discord.Modules;

public sealed class ToDoPriorityInteractionModule : ComponentInteractionModule<StringMenuInteractionContext>
{
  [ComponentInteraction(ToDoQuickCreateComponent.PrioritySelectCustomId)]
  public string HandlePriority() => $"Priority noted: {string.Join(", ", Context.SelectedValues)} (updates coming soon).";
}

public sealed class ToDoEnergyInteractionModule : ComponentInteractionModule<StringMenuInteractionContext>
{
  [ComponentInteraction(ToDoQuickCreateComponent.EnergySelectCustomId)]
  public string HandleEnergy() => $"Energy level noted: {string.Join(", ", Context.SelectedValues)} (updates coming soon).";
}

public sealed class ToDoInterestInteractionModule : ComponentInteractionModule<StringMenuInteractionContext>
{
  [ComponentInteraction(ToDoQuickCreateComponent.InterestSelectCustomId)]
  public string HandleInterest() => $"Interest noted: {string.Join(", ", Context.SelectedValues)} (updates coming soon).";
}

public sealed class ToDoReminderInteractionModule : ComponentInteractionModule<ButtonInteractionContext>
{
  [ComponentInteraction(ToDoQuickCreateComponent.ReminderButtonCustomId)]
  public string HandleReminderButton() => "Reminder editing is coming soon. For now, reply with a time and I'll log it!";
}

public sealed class ToDoEditInteractionModule : ComponentInteractionModule<ButtonInteractionContext>
{
  [ComponentInteraction(ToDoListComponent.EditButtonCustomId)]
  public string HandleEditButton()
  {
    return "Select which todo you'd like to edit via a select menu (coming soon!)";
  }
}

public sealed class ToDoDeleteInteractionModule : ComponentInteractionModule<ButtonInteractionContext>
{
  [ComponentInteraction(ToDoListComponent.DeleteButtonCustomId)]
  public string HandleDeleteButton()
  {
    return "Select which todo you'd like to delete via a select menu (coming soon!)";
  }
}
