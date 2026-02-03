using Apollo.Core.ToDos.Responses;

using NetCord;

namespace Apollo.Discord.Components;

/// <summary>
/// Component builder for displaying a list of to-dos with edit and delete options.
/// </summary>
public sealed class ToDoListComponent
{
  public const string EditButtonCustomId = "todo_edit_button";
  public const string DeleteButtonCustomId = "todo_delete_button";

  private readonly ComponentContainerProperties _container;

  public ToDoListComponent(IEnumerable<ToDoDTO> toDos, bool includeCompleted)
  {
    var todoList = string.Join("\n", toDos.Select(t => $"• **{t.Title}**"));

    _container = new ComponentContainerProperties
    {
      AccentColor = new Color(0x3B82F6),
      Components =
      [
        new TextDisplayProperties($"### Your To-Dos\n\n{(string.IsNullOrEmpty(todoList) ? "No to-dos found." : todoList)}"),
      ]
    };
  }

  public static implicit operator ComponentContainerProperties(ToDoListComponent component) => component._container;
  public static implicit operator ToDoListComponent(ComponentContainerProperties container) => new(container);

  private ToDoListComponent(ComponentContainerProperties container)
  {
    _container = container;
  }
}
