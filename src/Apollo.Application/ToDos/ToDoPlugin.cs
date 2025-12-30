using System.ComponentModel;

using Apollo.Application.ToDos.Commands;
using Apollo.Application.ToDos.Queries;
using Apollo.Core;
using Apollo.Core.People;
using Apollo.Core.ToDos;
using Apollo.Domain.People.ValueObjects;
using Apollo.Domain.ToDos.ValueObjects;

using Microsoft.SemanticKernel;

namespace Apollo.Application.ToDos;

public class ToDoPlugin(
  IMediator mediator,
  IPersonStore personStore,
  IFuzzyTimeParser fuzzyTimeParser,
  TimeProvider timeProvider,
  PersonConfig personConfig,
  PersonId personId)
{
  [KernelFunction("create_todo")]
  [Description("Creates a new todo with an optional reminder. Supports fuzzy times like 'in 10 minutes', 'in 2 hours', 'tomorrow', or ISO 8601 format.")]
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "Maintains better code readability")]
  public async Task<string> CreateToDoAsync(
    [Description("The todo description")] string description,
    [Description("Optional reminder time. Supports fuzzy formats like 'in 10 minutes', 'in 2 hours', 'tomorrow', 'next week', or ISO 8601 format (e.g., 2025-12-31T10:00:00).")] string? reminderDate = null)
  {
    try
    {
      DateTime? reminder = null;
      if (!string.IsNullOrEmpty(reminderDate))
      {
        // First, try to parse as fuzzy time (e.g., "in 10 minutes")
        var fuzzyResult = fuzzyTimeParser.TryParseFuzzyTime(reminderDate, timeProvider.GetUtcNow().UtcDateTime);
        if (fuzzyResult.IsSuccess)
        {
          reminder = fuzzyResult.Value;
        }
        else
        {
          // Fall back to ISO 8601 parsing
          if (!DateTime.TryParse(reminderDate, out var parsedDate))
          {
            return "Failed to create todo: Invalid reminder format. Use fuzzy time like 'in 10 minutes' or ISO 8601 format like 2025-12-31T10:00:00.";
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
      }

      var command = new CreateToDoCommand(
        personId,
        new Description(description),
        reminder
      );

      var result = await mediator.Send(command);

      if (result.IsFailed)
      {
        return $"Failed to create todo: {result.GetErrorMessages()}";
      }

      return reminder.HasValue
        ? $"Successfully created todo '{result.Value.Description.Value}' with a reminder set for {reminder.Value:yyyy-MM-dd HH:mm:ss} UTC."
        : $"Successfully created todo '{result.Value.Description.Value}'.";
    }
    catch (Exception ex)
    {
      return $"Error creating todo: {ex.Message}";
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
        return "Failed to update todo: Invalid todo ID format. The ID must be a valid GUID.";
      }

      var command = new UpdateToDoCommand(
        new ToDoId(todoGuid),
        new Description(description)
      );

      var result = await mediator.Send(command);

      return result.IsFailed ? $"Failed to update todo: {result.GetErrorMessages()}" : $"Successfully updated the todo to '{description}'.";
    }
    catch (Exception ex)
    {
      return $"Error updating todo: {ex.Message}";
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
        return "Failed to complete todo: Invalid todo ID format. The ID must be a valid GUID.";
      }

      var command = new CompleteToDoCommand(new ToDoId(todoGuid));
      var result = await mediator.Send(command);

      return result.IsFailed ? $"Failed to complete todo: {result.GetErrorMessages()}" : "Successfully marked the todo as completed.";
    }
    catch (Exception ex)
    {
      return $"Error completing todo: {ex.Message}";
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
        return "Failed to delete todo: Invalid todo ID format. The ID must be a valid GUID.";
      }

      var command = new DeleteToDoCommand(new ToDoId(todoGuid));
      var result = await mediator.Send(command);

      return result.IsFailed ? $"Failed to delete todo: {result.GetErrorMessages()}" : "Successfully deleted the todo.";
    }
    catch (Exception ex)
    {
      return $"Error deleting todo: {ex.Message}";
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
        return $"Failed to retrieve todos: {result.GetErrorMessages()}";
      }

      if (!result.Value.Any())
      {
        return "You currently have no active todos.";
      }

      var todoList = result.Value.Select((t, index) =>
        $"{index + 1}. {t.Description.Value} (ID: {t.Id.Value}, Created: {t.CreatedOn.Value:yyyy-MM-dd})"
      );

      return $"Here are your active todos:\n{string.Join("\n", todoList)}";
    }
    catch (Exception ex)
    {
      return $"Error retrieving todos: {ex.Message}";
    }
  }
}
