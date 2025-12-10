using System.ComponentModel;
using System.Text.Json;

using Apollo.Domain.People.ValueObjects;
using Apollo.Domain.ToDos.ValueObjects;

using Microsoft.SemanticKernel;

namespace Apollo.Application.ToDos;

public class ToDoPlugin(IMediator mediator, PersonId personId)
{
  [KernelFunction("create_todo")]
  [Description("Creates a new todo with an optional reminder date")]
  public async Task<string> CreateToDoAsync(
    [Description("The todo description")] string description,
    [Description("Optional reminder date in ISO 8601 format (e.g., 2025-12-31T10:00:00Z)")] string? reminderDate = null)
  {
    try
    {
      DateTime? reminder = null;
      if (!string.IsNullOrEmpty(reminderDate))
      {
        if (!DateTime.TryParse(reminderDate, out var parsedDate))
        {
          return JsonSerializer.Serialize(new { success = false, error = "Invalid reminder date format" });
        }
        reminder = parsedDate;
      }

      var command = new CreateToDoCommand(
        personId,
        new Description(description),
        reminder
      );

      var result = await mediator.Send(command);

      if (result.IsFailed)
      {
        return JsonSerializer.Serialize(new { success = false, error = string.Join(", ", result.Errors.Select(e => e.Message)) });
      }

      return JsonSerializer.Serialize(new
      {
        success = true,
        todoId = result.Value.Id.Value,
        description = result.Value.Description.Value,
        createdOn = result.Value.CreatedOn.Value
      });
    }
    catch (Exception ex)
    {
      return JsonSerializer.Serialize(new { success = false, error = ex.Message });
    }
  }

  [KernelFunction("update_todo")]
  [Description("Updates an existing todo's description")]
  public async Task<string> UpdateToDoAsync(
    [Description("The todo ID (GUID)")] string todoId,
    [Description("The new todo description")] string description)
  {
    try
    {
      if (!Guid.TryParse(todoId, out var todoGuid))
      {
        return JsonSerializer.Serialize(new { success = false, error = "Invalid todo ID format" });
      }

      var command = new UpdateToDoCommand(
        new ToDoId(todoGuid),
        new Description(description)
      );

      var result = await mediator.Send(command);

      if (result.IsFailed)
      {
        return JsonSerializer.Serialize(new { success = false, error = string.Join(", ", result.Errors.Select(e => e.Message)) });
      }

      return JsonSerializer.Serialize(new { success = true, message = "Todo updated successfully" });
    }
    catch (Exception ex)
    {
      return JsonSerializer.Serialize(new { success = false, error = ex.Message });
    }
  }

  [KernelFunction("complete_todo")]
  [Description("Marks a todo as completed")]
  public async Task<string> CompleteToDoAsync(
    [Description("The todo ID (GUID)")] string todoId)
  {
    try
    {
      if (!Guid.TryParse(todoId, out var todoGuid))
      {
        return JsonSerializer.Serialize(new { success = false, error = "Invalid todo ID format" });
      }

      var command = new CompleteToDoCommand(new ToDoId(todoGuid));
      var result = await mediator.Send(command);

      if (result.IsFailed)
      {
        return JsonSerializer.Serialize(new { success = false, error = string.Join(", ", result.Errors.Select(e => e.Message)) });
      }

      return JsonSerializer.Serialize(new { success = true, message = "Todo completed successfully" });
    }
    catch (Exception ex)
    {
      return JsonSerializer.Serialize(new { success = false, error = ex.Message });
    }
  }

  [KernelFunction("delete_todo")]
  [Description("Deletes a todo")]
  public async Task<string> DeleteToDoAsync(
    [Description("The todo ID (GUID)")] string todoId)
  {
    try
    {
      if (!Guid.TryParse(todoId, out var todoGuid))
      {
        return JsonSerializer.Serialize(new { success = false, error = "Invalid todo ID format" });
      }

      var command = new DeleteToDoCommand(new ToDoId(todoGuid));
      var result = await mediator.Send(command);

      if (result.IsFailed)
      {
        return JsonSerializer.Serialize(new { success = false, error = string.Join(", ", result.Errors.Select(e => e.Message)) });
      }

      return JsonSerializer.Serialize(new { success = true, message = "Todo deleted successfully" });
    }
    catch (Exception ex)
    {
      return JsonSerializer.Serialize(new { success = false, error = ex.Message });
    }
  }

  [KernelFunction("list_todos")]
  [Description("Lists all active todos for the current person")]
  public async Task<string> ListToDosAsync()
  {
    try
    {
      var query = new GetToDosByPersonIdQuery(personId);
      var result = await mediator.Send(query);

      if (result.IsFailed)
      {
        return JsonSerializer.Serialize(new { success = false, error = string.Join(", ", result.Errors.Select(e => e.Message)) });
      }

      var todos = result.Value.Select(t => new
      {
        todoId = t.Id.Value,
        description = t.Description.Value,
        createdOn = t.CreatedOn.Value,
        updatedOn = t.UpdatedOn.Value
      });

      return JsonSerializer.Serialize(new { success = true, todos });
    }
    catch (Exception ex)
    {
      return JsonSerializer.Serialize(new { success = false, error = ex.Message });
    }
  }
}
