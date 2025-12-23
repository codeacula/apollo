using System.ComponentModel;
using System.Text.Json;

using Apollo.Application.ToDos.Commands;
using Apollo.Application.ToDos.Queries;
using Apollo.Core.People;
using Apollo.Domain.People.ValueObjects;
using Apollo.Domain.ToDos.ValueObjects;

using Microsoft.SemanticKernel;

namespace Apollo.Application.ToDos;

public class ToDoPlugin(IMediator mediator, IPersonStore personStore, PersonConfig personConfig, PersonId personId)
{
  [KernelFunction("create_todo")]
  [Description("Creates a new todo with an optional reminder date. Reminder times are interpreted in the user's timezone.")]
  public async Task<string> CreateToDoAsync(
    [Description("The todo description")] string description,
    [Description("Optional reminder date in ISO 8601 format (e.g., 2025-12-31T10:00:00). Time is interpreted in your local timezone.")] string? reminderDate = null)
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

        // Get user's timezone or use default
        var personResult = await personStore.GetAsync(personId);
        var timeZoneId = personResult.IsSuccess && personResult.Value.TimeZoneId.HasValue
          ? personResult.Value.TimeZoneId.Value.Value
          : personConfig.DefaultTimeZoneId;

        var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

        // Convert from user's local time to UTC
        if (parsedDate.Kind == DateTimeKind.Unspecified)
        {
          reminder = TimeZoneInfo.ConvertTimeToUtc(parsedDate, timeZoneInfo);
        }
        else if (parsedDate.Kind == DateTimeKind.Local)
        {
          // If it's already specified as local, treat it as system local time and convert directly to UTC
          reminder = parsedDate.ToUniversalTime();
        }
        else
        {
          // Already UTC
          reminder = parsedDate;
        }
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

      return JsonSerializer.Serialize(new { success = true, message = "ToDo updated successfully" });
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

      return JsonSerializer.Serialize(new { success = true, message = "ToDo completed successfully" });
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

      return JsonSerializer.Serialize(new { success = true, message = "ToDo deleted successfully" });
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
