using Apollo.Application.Conversations;
using Apollo.Application.People.Queries;
using Apollo.Application.ToDos.Commands;
using Apollo.Application.ToDos.Queries;
using Apollo.Core.People;
using Apollo.Core.ToDos;
using Apollo.Domain.Common.Enums;
using Apollo.Domain.People.ValueObjects;
using Apollo.Domain.ToDos.ValueObjects;
using Apollo.GRPC.Contracts;

using MediatR;

using CoreNewMessageRequest = Apollo.Core.Conversations.NewMessageRequest;

namespace Apollo.GRPC.Service;

public sealed class ApolloGrpcService(
  IMediator mediator,
  IReminderStore reminderStore,
  IPersonStore personStore,
  SuperAdminConfig superAdminConfig
) : IApolloGrpcService
{
  public async Task<GrpcResult<string>> SendApolloMessageAsync(NewMessageRequest message)
  {
    var coreRequest = new CoreNewMessageRequest
    {
      PlatformId = message.ToPlatformId(),
      Content = message.Content
    };

    var requestResult = await mediator.Send(new ProcessIncomingMessageCommand(coreRequest));
    return requestResult.IsSuccess ?
      requestResult.Value.Content.Value :
      requestResult.Errors.Select(e => new GrpcError(e.Message)).ToArray();
  }

  public async Task<GrpcResult<ToDoDTO>> CreateToDoAsync(CreateToDoRequest request)
  {
    var platformId = request.ToPlatformId();
    var personResult = await mediator.Send(new GetOrCreatePersonByPlatformIdQuery(platformId));

    if (personResult.IsFailed)
    {
      return personResult.Errors.Select(e => new GrpcError(e.Message)).ToArray();
    }

    var command = new CreateToDoCommand(
      personResult.Value.Id,
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
      UpdatedOn = todo.UpdatedOn.Value
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
      UpdatedOn = todo.UpdatedOn.Value
    };
  }

  public async Task<GrpcResult<ToDoDTO[]>> GetPersonToDosAsync(GetPersonToDosRequest request)
  {
    // First, resolve PlatformUserId to PersonId
    var platformId = new PlatformId(request.Username, request.PlatformUserId, request.Platform);
    var personResult = await mediator.Send(new GetOrCreatePersonByPlatformIdQuery(platformId));

    if (personResult.IsFailed)
    {
      return personResult.Errors.Select(e => new GrpcError(e.Message)).ToArray();
    }

    var query = new GetToDosByPersonIdQuery(personResult.Value.Id, request.IncludeCompleted);
    var result = await mediator.Send(query);

    if (result.IsFailed)
    {
      return result.Errors.Select(e => new GrpcError(e.Message)).ToArray();
    }

    var toDos = result.Value;
    var dtoTasks = toDos.Select(async t =>
    {
      DateTime? reminderDate = null;
      var remindersResult = await reminderStore.GetByToDoIdAsync(t.Id);

      if (remindersResult.IsSuccess)
      {
        var reminderTimes = remindersResult.Value
          .Select(r => r.ReminderTime.Value)
          .Order()
          .ToList();

        var upcoming = reminderTimes.FirstOrDefault(d => d >= DateTime.UtcNow);
        reminderDate = upcoming != default ? upcoming : reminderTimes.FirstOrDefault();
      }

      return new ToDoDTO
      {
        Id = t.Id.Value,
        PersonId = t.PersonId.Value,
        Description = t.Description.Value,
        ReminderDate = reminderDate,
        CreatedOn = t.CreatedOn.Value,
        UpdatedOn = t.UpdatedOn.Value
      };
    });

    return await Task.WhenAll(dtoTasks);
  }

  public async Task<GrpcResult<DailyPlanDTO>> GetDailyPlanAsync(GetDailyPlanRequest request)
  {
    // Resolve PlatformUserId to PersonId
    var platformId = new PlatformId(request.Username, request.PlatformUserId, request.Platform);
    var personResult = await mediator.Send(new GetOrCreatePersonByPlatformIdQuery(platformId));

    if (personResult.IsFailed)
    {
      return personResult.Errors.Select(e => new GrpcError(e.Message)).ToArray();
    }

    var query = new GetDailyPlanQuery(personResult.Value.Id);
    var result = await mediator.Send(query);

    if (result.IsFailed)
    {
      return result.Errors.Select(e => new GrpcError(e.Message)).ToArray();
    }

    var plan = result.Value;
    var taskDtos = plan.SuggestedTasks.Select(t => new DailyPlanTaskDTO
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
    // Verify the requester is a super admin
    if (!IsSuperAdmin(request.AdminPlatform, request.AdminPlatformUserId))
    {
      return new GrpcError("Only super admins can grant access", "UNAUTHORIZED");
    }

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
    // Verify the requester is a super admin
    if (!IsSuperAdmin(request.AdminPlatform, request.AdminPlatformUserId))
    {
      return new GrpcError("Only super admins can revoke access", "UNAUTHORIZED");
    }

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
