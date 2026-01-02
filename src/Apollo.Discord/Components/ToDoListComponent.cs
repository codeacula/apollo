using Apollo.Core.ToDos.Responses;

using NetCord;
using NetCord.Rest;

namespace Apollo.Discord.Components;

public class ToDoListComponent : ComponentContainerProperties
{
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

      // Add edit and delete buttons for this todo
      components.Add(new ActionRowProperties
      {
        Components =
        [
          new ButtonProperties($"todo_edit_{todo.Id}", "Edit", ButtonStyle.Secondary),
          new ButtonProperties($"todo_delete_{todo.Id}", "Delete", ButtonStyle.Danger),
        ]
      });
    }

    Components = components;
  }
}
