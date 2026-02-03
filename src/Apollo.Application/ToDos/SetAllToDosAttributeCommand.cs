using Apollo.Core;
using Apollo.Core.Logging;
using Apollo.Core.ToDos;
using Apollo.Domain.People.ValueObjects;
using Apollo.Domain.ToDos.ValueObjects;

using FluentResults;

namespace Apollo.Application.ToDos;

public sealed record SetAllToDosAttributeCommand(
  PersonId PersonId,
  IReadOnlyList<ToDoId> ToDoIds,
  Priority? Priority = null,
  Energy? Energy = null,
  Interest? Interest = null
) : IRequest<Result<int>>;

public sealed class SetAllToDosAttributeCommandHandler(
  IToDoStore toDoStore,
  ILogger<SetAllToDosAttributeCommandHandler> logger) : IRequestHandler<SetAllToDosAttributeCommand, Result<int>>
{
  public async Task<Result<int>> Handle(SetAllToDosAttributeCommand request, CancellationToken cancellationToken)
  {
    try
    {
      var todoIds = request.ToDoIds;

      // If empty list, fetch all active todos for the person
      if (todoIds.Count == 0)
      {
        var allTodosResult = await toDoStore.GetByPersonIdAsync(request.PersonId, includeCompleted: false, cancellationToken);
        if (allTodosResult.IsFailed)
        {
          return Result.Fail<int>(allTodosResult.GetErrorMessages());
        }

        todoIds = [.. allTodosResult.Value.Select(t => t.Id)];
      }

      var updatedCount = 0;
      var errors = new List<string>();

      foreach (var todoId in todoIds)
      {
        try
        {
          // Verify ownership
          var todoResult = await toDoStore.GetAsync(todoId, cancellationToken);
          if (todoResult.IsFailed)
          {
            errors.Add($"To-Do {todoId.Value}: Not found");
            continue;
          }

          if (todoResult.Value.PersonId.Value != request.PersonId.Value)
          {
            errors.Add($"To-Do {todoId.Value}: Permission denied");
            continue;
          }

          // Update each specified attribute
          var todoUpdated = false;

          if (request.Priority.HasValue)
          {
            var result = await toDoStore.UpdatePriorityAsync(todoId, request.Priority.Value, cancellationToken);
            if (result.IsFailed)
            {
              errors.Add($"To-Do {todoId.Value} priority: {result.GetErrorMessages()}");
            }
            else
            {
              todoUpdated = true;
            }
          }

          if (request.Energy.HasValue)
          {
            var result = await toDoStore.UpdateEnergyAsync(todoId, request.Energy.Value, cancellationToken);
            if (result.IsFailed)
            {
              errors.Add($"To-Do {todoId.Value} energy: {result.GetErrorMessages()}");
            }
            else
            {
              todoUpdated = true;
            }
          }

          if (request.Interest.HasValue)
          {
            var result = await toDoStore.UpdateInterestAsync(todoId, request.Interest.Value, cancellationToken);
            if (result.IsFailed)
            {
              errors.Add($"To-Do {todoId.Value} interest: {result.GetErrorMessages()}");
            }
            else
            {
              todoUpdated = true;
            }
          }

          if (todoUpdated)
          {
            updatedCount++;
          }
        }
        catch (Exception ex)
        {
          errors.Add($"To-Do {todoId.Value}: {ex.Message}");
        }
      }

      if (errors.Count > 0)
      {
        // Log errors but still return success with count
        foreach (var error in errors)
        {
          ToDoLogs.LogBulkUpdateError(logger, error);
        }
      }

      return Result.Ok(updatedCount);
    }
    catch (Exception ex)
    {
      return Result.Fail<int>(ex.Message);
    }
  }
}
