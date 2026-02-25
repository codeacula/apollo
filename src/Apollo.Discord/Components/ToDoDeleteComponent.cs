using Apollo.Core.ToDos.Responses;

using NetCord.Rest;

namespace Apollo.Discord.Components;

public class ToDoDeleteComponent : ComponentContainerProperties
{
  public const string SelectCustomId = "todo_delete_select";

  public ToDoDeleteComponent(IEnumerable<ToDoSummary> todos)
  {
    AccentColor = Constants.Colors.Error;

    var todoList = todos.ToList();

    var options = todoList.ConvertAll(t => new StringMenuSelectOptionProperties(
      t.Description.Length > 100 ? t.Description[..100] : t.Description,
      t.Id.ToString()
    ));

    Components =
    [
      new TextDisplayProperties("## Delete a To-Do"),
      new TextDisplayProperties("Select the to-do you want to delete:"),
      new StringMenuProperties(SelectCustomId, options)
      {
        Placeholder = "Choose a to-do to delete...",
      }
    ];
  }
}
