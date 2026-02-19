using Apollo.Application.Conversations;
using Apollo.Application.People;
using Apollo.Application.ToDos;
using Apollo.Core.People;
using Apollo.Core.ToDos;
using Apollo.Domain.Common.Enums;
using Apollo.Domain.ToDos.Models;
using Apollo.Domain.ToDos.ValueObjects;
using Apollo.GRPC.Context;
using Apollo.GRPC.Contracts;

using MediatR;

namespace Apollo.GRPC.Service;

public sealed class ApolloGrpcService(
  IMediator mediator,
  IReminderStore reminderStore,
  IPersonStore personStore,
  ITimeParsingService timeParsingService,
  SuperAdminConfig superAdminConfig,
  IUserContext userContext
) : IApolloGrpcService
{
  public async Task<GrpcResult<string>> SendApolloMessageAsync(NewMessageRequest message)
  {
    var person = userContext.Person!;
    var command = new ProcessIncomingMessageCommand(person.Id, new Domain.Common.ValueObjects.Content(message.Content));

    var requestResult = await mediator.Send(command);
    return requestResult.IsFailed
      ? (GrpcResult<string>)requestResult.Errors.Select(e => new GrpcError(e.Message)).ToArray()
      : (GrpcResult<string>)requestResult.Value.Content.Value;
  }

  public async Task<GrpcResult<ToDoDTO>> CreateToDoAsync(CreateToDoRequest request)
  {
    var person = userContext.Person!;

    var command = new CreateToDoCommand(
      person.Id,
      new Description(request.Description),
      request.ReminderDate
    );

    var result = await mediator.Send(command);

    if (result.IsFailed)
    {
      return result.Errors.Select(e => new GrpcError(e.Message)).ToArray();
    }

    var todo = result.Value;
    return new ToDoDTO
    {
      Id = todo.Id.Value,
      PersonId = todo.PersonId.Value,
      Description = todo.Description.Value,
      ReminderDate = request.ReminderDate,
      CreatedOn = todo.CreatedOn.Value,
      UpdatedOn = todo.UpdatedOn.Value,
      Priority = todo.Priority.Value,
      Energy = todo.Energy.Value,
      Interest = todo.Interest.Value
    };
  }

  public async Task<GrpcResult<ReminderDTO>> CreateReminderAsync(CreateReminderRequest request)
  {
    var person = userContext.Person!;

    // Parse the reminder time using the consolidated time parsing service
    var userTimeZoneId = person.TimeZoneId.HasValue ? person.TimeZoneId.Value.Value : null;
    var parsedTimeResult = await timeParsingService.ParseTimeAsync(request.ReminderTime, userTimeZoneId);
    if (parsedTimeResult.IsFailed)
    {
      return parsedTimeResult.Errors.Select(e => new GrpcError(e.Message)).ToArray();
    }

    var command = new CreateReminderCommand(
      person.Id,
      request.Message,
      parsedTimeResult.Value
    );

    var result = await mediator.Send(command);

    if (result.IsFailed)
    {
      return result.Errors.Select(e => new GrpcError(e.Message)).ToArray();
    }

    var reminder = result.Value;
    return new ReminderDTO
    {
      Id = reminder.Id.Value,
      PersonId = reminder.PersonId.Value,
      Details = reminder.Details.Value,
      ReminderTime = reminder.ReminderTime.Value,
      CreatedOn = reminder.CreatedOn.Value,
      UpdatedOn = reminder.UpdatedOn.Value
    };
  }

  public async Task<GrpcResult<ToDoDTO>> GetToDoAsync(GetToDoRequest request)
  {
    var query = new GetToDoByIdQuery(new ToDoId(request.ToDoId));
    var result = await mediator.Send(query);

    if (result.IsFailed)
    {
      return result.Errors.Select(e => new GrpcError(e.Message)).ToArray();
    }

    var todo = result.Value;
    return new ToDoDTO
    {
      Id = todo.Id.Value,
      PersonId = todo.PersonId.Value,
      Description = todo.Description.Value,
      CreatedOn = todo.CreatedOn.Value,
      UpdatedOn = todo.UpdatedOn.Value,
      Priority = todo.Priority.Value,
      Energy = todo.Energy.Value,
      Interest = todo.Interest.Value
    };
  }

  public async Task<GrpcResult<ToDoDTO[]>> GetPersonToDosAsync(GetPersonToDosRequest request)
  {
    var person = userContext.Person!;

    var query = new GetToDosByPersonIdQuery(person.Id, request.IncludeCompleted);
    var result = await mediator.Send(query);

    if (result.IsFailed)
    {
      return result.Errors.Select(e => new GrpcError(e.Message)).ToArray();
    }

    var toDos = result.Value;
    var dtoTasks = toDos.Select(async t =>
    {
      var remindersResult = await reminderStore.GetByToDoIdAsync(t.Id);
      if (remindersResult.IsFailed)
      {
        return CreateDto(t, null);
      }

      var reminderTimes = remindersResult.Value
        .Select(r => r.ReminderTime.Value)
        .Order()
        .ToList();

      var upcoming = reminderTimes.FirstOrDefault(d => d >= DateTime.UtcNow);
      var reminderDate = upcoming != default ? upcoming : reminderTimes.FirstOrDefault();

      return CreateDto(t, reminderDate);
    });

    return await Task.WhenAll(dtoTasks);
  }

  private static ToDoDTO CreateDto(ToDo t, DateTime? reminderDate)
  {
    return new ToDoDTO
    {
      Id = t.Id.Value,
      PersonId = t.PersonId.Value,
      Description = t.Description.Value,
      ReminderDate = reminderDate,
      CreatedOn = t.CreatedOn.Value,
      UpdatedOn = t.UpdatedOn.Value,
      Priority = t.Priority.Value,
      Energy = t.Energy.Value,
      Interest = t.Interest.Value
    };
  }

  public async Task<GrpcResult<DailyPlanDTO>> GetDailyPlanAsync(GetDailyPlanRequest request)
  {
    var person = userContext.Person!;

    var query = new GetDailyPlanQuery(person.Id);
    var result = await mediator.Send(query);

    if (result.IsFailed)
    {
      return result.Errors.Select(e => new GrpcError(e.Message)).ToArray();
    }

    var plan = result.Value;
    // Defensive: ensure SuggestedTasks is non-null to avoid serialization edge-cases where
    // an empty array might deserialize as null on the client.
    var safeSuggested = (IReadOnlyList<Application.ToDos.Models.DailyPlanItem>?)plan.SuggestedTasks ?? [];

    var taskDtos = safeSuggested.Select(t => new DailyPlanTaskDTO
    {
      Id = t.Id.Value,
      Description = t.Description,
      Priority = (int)t.Priority.Value,
      Energy = (int)t.Energy.Value,
      Interest = (int)t.Interest.Value,
      DueDate = t.DueDate
    }).ToArray();

    return new DailyPlanDTO
    {
      SuggestedTasks = taskDtos,
      SelectionRationale = plan.SelectionRationale,
      TotalActiveTodos = plan.TotalActiveTodos
    };
  }

  public async Task<GrpcResult<string>> UpdateToDoAsync(UpdateToDoRequest request)
  {
    var command = new UpdateToDoCommand(
      new ToDoId(request.ToDoId),
      new Description(request.Description)
    );

    var result = await mediator.Send(command);
    return result.IsFailed ? (GrpcResult<string>)result.Errors.Select(e => new GrpcError(e.Message)).ToArray() : (GrpcResult<string>)"ToDo updated successfully";
  }

  public async Task<GrpcResult<string>> CompleteToDoAsync(CompleteToDoRequest request)
  {
    var command = new CompleteToDoCommand(new ToDoId(request.ToDoId));
    var result = await mediator.Send(command);

    return result.IsFailed ? (GrpcResult<string>)result.Errors.Select(e => new GrpcError(e.Message)).ToArray() : (GrpcResult<string>)"ToDo completed successfully";
  }

  public async Task<GrpcResult<string>> DeleteToDoAsync(DeleteToDoRequest request)
  {
    var command = new DeleteToDoCommand(new ToDoId(request.ToDoId));
    var result = await mediator.Send(command);

    return result.IsFailed ? (GrpcResult<string>)result.Errors.Select(e => new GrpcError(e.Message)).ToArray() : (GrpcResult<string>)"ToDo deleted successfully";
  }

  public async Task<GrpcResult<string>> GrantAccessAsync(ManageAccessRequest request)
  {
    // Get or create the target user
    var targetPlatformId = request.ToTargetPlatformId();
    var personResult = await mediator.Send(new GetOrCreatePersonByPlatformIdQuery(targetPlatformId));

    if (personResult.IsFailed)
    {
      return personResult.Errors.Select(e => new GrpcError(e.Message)).ToArray();
    }

    // Grant access to the target user
    var grantResult = await personStore.GrantAccessAsync(personResult.Value.Id);

    return grantResult.IsFailed
      ? (GrpcResult<string>)grantResult.Errors.Select(e => new GrpcError(e.Message)).ToArray()
      : (GrpcResult<string>)$"Access granted to {request.TargetUsername}";
  }

  public async Task<GrpcResult<string>> RevokeAccessAsync(ManageAccessRequest request)
  {
    // Get the target user
    var targetPlatformId = request.ToTargetPlatformId();
    var personResult = await personStore.GetByPlatformIdAsync(targetPlatformId);

    if (personResult.IsFailed)
    {
      return personResult.Errors.Select(e => new GrpcError(e.Message)).ToArray();
    }

    // Prevent revoking super admin's own access
    if (IsSuperAdmin(request.TargetPlatform, request.TargetPlatformUserId))
    {
      return new GrpcError("Cannot revoke access from the super admin", "FORBIDDEN");
    }

    // Revoke access from the target user
    var revokeResult = await personStore.RevokeAccessAsync(personResult.Value.Id);

    return revokeResult.IsFailed
      ? (GrpcResult<string>)revokeResult.Errors.Select(e => new GrpcError(e.Message)).ToArray()
      : (GrpcResult<string>)$"Access revoked from {request.TargetUsername}";
  }

  private bool IsSuperAdmin(Platform platform, string platformUserId)
  {
    return !string.IsNullOrWhiteSpace(superAdminConfig.DiscordUserId)
      && platform == Platform.Discord
      && string.Equals(platformUserId, superAdminConfig.DiscordUserId, StringComparison.OrdinalIgnoreCase);
  }
}
