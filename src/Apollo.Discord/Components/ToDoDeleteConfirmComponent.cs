using NetCord;
using NetCord.Rest;

namespace Apollo.Discord.Components;

public class ToDoDeleteConfirmComponent : ComponentContainerProperties
{
  public const string ConfirmButtonCustomId = "todo_delete_confirm";
  public const string CancelButtonCustomId = "todo_delete_cancel";

  public ToDoDeleteConfirmComponent(Guid toDoId, string description)
  {
    AccentColor = Constants.Colors.Error;

    Components =
    [
      new TextDisplayProperties("## Confirm Delete"),
      new TextDisplayProperties($"Are you sure you want to delete **{description}**? This cannot be undone."),
      new ActionRowProperties
      {
        Components =
        [
          new ButtonProperties($"{ConfirmButtonCustomId}:{toDoId}", "Delete", ButtonStyle.Danger),
          new ButtonProperties(CancelButtonCustomId, "Cancel", ButtonStyle.Secondary),
        ]
      }
    ];
  }
}
