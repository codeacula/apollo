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
      var idsResult = await ResolveToDoIdsAsync(request.PersonId, request.ToDoIds, cancellationToken);
      if (idsResult.IsFailed)
      {
        return idsResult.ToResult<int>();
      }

      var updatedCount = 0;
      var allErrors = new List<string>();

      foreach (var todoId in idsResult.Value)
      {
        var ownershipResult = await VerifyOwnershipAsync(todoId, request.PersonId, cancellationToken);
        if (ownershipResult.IsFailed)
        {
          allErrors.Add($"To-Do {todoId.Value}: {ownershipResult.GetErrorMessages()}");
          continue;
        }

        var (updated, errors) = await UpdateAttributesAsync(todoId, request, cancellationToken);
        if (updated)
        {
          updatedCount++;
        }

        allErrors.AddRange(errors);
      }

      LogErrors(allErrors);
      return Result.Ok(updatedCount);
    }
    catch (Exception ex)
    {
      return Result.Fail<int>(ex.Message);
    }
  }

  private async Task<Result<IReadOnlyList<ToDoId>>> ResolveToDoIdsAsync(
    PersonId personId,
    IReadOnlyList<ToDoId> toDoIds,
    CancellationToken cancellationToken)
  {
    if (toDoIds.Count == 0)
    {
      var allTodosResult = await toDoStore.GetByPersonIdAsync(personId, includeCompleted: false, cancellationToken);
      return allTodosResult.IsFailed
        ? Result.Fail<IReadOnlyList<ToDoId>>(allTodosResult.GetErrorMessages())
        : Result.Ok<IReadOnlyList<ToDoId>>([.. allTodosResult.Value.Select(t => t.Id)]);
    }

    return Result.Ok(toDoIds);
  }

  private async Task<Result> VerifyOwnershipAsync(
    ToDoId toDoId,
    PersonId personId,
    CancellationToken cancellationToken)
  {
    var todoResult = await toDoStore.GetAsync(toDoId, cancellationToken);
    if (todoResult.IsFailed)
    {
      return Result.Fail("Not found");
    }

    return todoResult.Value.PersonId.Value != personId.Value ? Result.Fail("Permission denied") : Result.Ok();
  }

  private async Task<(bool Updated, List<string> Errors)> UpdateAttributesAsync(
    ToDoId toDoId,
    SetAllToDosAttributeCommand request,
    CancellationToken cancellationToken)
  {
    var updated = false;
    var errors = new List<string>();

    if (request.Priority.HasValue)
    {
      var result = await toDoStore.UpdatePriorityAsync(toDoId, request.Priority.Value, cancellationToken);
      if (result.IsFailed)
      {
        errors.Add($"To-Do {toDoId.Value} priority: {result.GetErrorMessages()}");
      }
      else
      {
        updated = true;
      }
    }

    if (request.Energy.HasValue)
    {
      var result = await toDoStore.UpdateEnergyAsync(toDoId, request.Energy.Value, cancellationToken);
      if (result.IsFailed)
      {
        errors.Add($"To-Do {toDoId.Value} energy: {result.GetErrorMessages()}");
      }
      else
      {
        updated = true;
      }
    }

    if (request.Interest.HasValue)
    {
      var result = await toDoStore.UpdateInterestAsync(toDoId, request.Interest.Value, cancellationToken);
      if (result.IsFailed)
      {
        errors.Add($"To-Do {toDoId.Value} interest: {result.GetErrorMessages()}");
      }
      else
      {
        updated = true;
      }
    }

    return (updated, errors);
  }

  private void LogErrors(List<string> errors)
  {
    if (errors.Count > 0)
    {
      foreach (var error in errors)
      {
        ToDoLogs.LogBulkUpdateError(logger, error);
      }
    }
  }
}
