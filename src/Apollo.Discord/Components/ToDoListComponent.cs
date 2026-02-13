using Apollo.Core.ToDos.Responses;

using NetCord;
using NetCord.Rest;

namespace Apollo.Discord.Components;

public sealed class ToDoListComponent : ComponentContainerProperties
{
  public const string EditButtonCustomId = "todo_list_edit";
  public const string DeleteButtonCustomId = "todo_list_delete";

  public ToDoListComponent(IEnumerable<ToDoSummary> todos, bool includeCompleted)
  {
    AccentColor = Constants.Colors.ApolloGreen;

    var components = new List<IComponentContainerComponentProperties>();
    var listType = includeCompleted ? "all to-do(s)" : "active to-do(s)";
    components.Add(new TextDisplayProperties($"# Your {listType}"));

    var todoList = todos.ToList();
    if (todoList.Count == 0)
    {
      var emptyMessage = includeCompleted
        ? "You have no to-dos at all. 🎉"
        : "You have no active to-dos. 🎉";
      components.Add(new TextDisplayProperties(emptyMessage));
      Components = components;
      return;
    }

    // Create a table-like structure with each todo as a row
    foreach (var todo in todoList)
    {
      var reminderText = todo.ReminderDate.HasValue
        ? $" | 🔔 <t:{new DateTimeOffset(todo.ReminderDate.Value).ToUnixTimeSeconds()}:R>"
        : string.Empty;

      components.Add(new TextDisplayProperties($"**{todo.Description}**{reminderText}"));
    }

    // Add action buttons at the bottom
    components.Add(new ActionRowProperties
    {
      Components =
      [
        new ButtonProperties(EditButtonCustomId, "Edit Todo", ButtonStyle.Secondary),
        new ButtonProperties(DeleteButtonCustomId, "Delete Todo", ButtonStyle.Danger),
      ]
    });

    Components = components;
  }
}
