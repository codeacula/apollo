using Apollo.Application.Conversations;
using Apollo.Application.People.Handlers;
using Apollo.Application.People.Queries;
using Apollo.Application.ToDos.Commands;
using Apollo.Application.ToDos.Queries;
using Apollo.Core.Conversations;
using Apollo.Core.ToDos;
using Apollo.Domain.People.ValueObjects;
using Apollo.Domain.ToDos.ValueObjects;
using Apollo.GRPC.Contracts;

using MediatR;

namespace Apollo.GRPC.Service;

public sealed class ApolloGrpcService(
  IMediator mediator,
  IReminderStore reminderStore
) : IApolloGrpcService
{
  public async Task<GrpcResult<string>> SendApolloMessageAsync(NewMessageRequest message)
  {
    var requestResult = await mediator.Send(new ProcessIncomingMessageCommand(message));
    return requestResult.IsSuccess ?
      requestResult.Value.Content.Value :
      requestResult.Errors.Select(e => new GrpcError(e.Message)).ToArray();
  }

  public async Task<GrpcResult<ToDoDTO>> CreateToDoAsync(CreateToDoRequest request)
  {
    var username = new Username();
    var personId = new PlatformId(request.Username, request.PlatformId, request.Platform);
    var personResult = await mediator.Send(new GetOrCreatePersonByPlatformIdQuery(personId, username));

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
      PersonPlatform = todo.PersonId.Platform,
      PersonProviderId = todo.PersonId.ProviderId,
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
      PersonPlatform = todo.PersonId.Platform,
      PersonProviderId = todo.PersonId.ProviderId,
      Description = todo.Description.Value,
      CreatedOn = todo.CreatedOn.Value,
      UpdatedOn = todo.UpdatedOn.Value
    };
  }

  public async Task<GrpcResult<ToDoDTO[]>> GetPersonToDosAsync(GetPersonToDosRequest request)
  {
    var personId = new PersonId(request.Platform, request.PlatformUserId);
    var query = new GetToDosByPersonIdQuery(personId, request.IncludeCompleted);
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
        PersonPlatform = t.PersonId.Platform,
        PersonProviderId = t.PersonId.ProviderId,
        Description = t.Description.Value,
        ReminderDate = reminderDate,
        CreatedOn = t.CreatedOn.Value,
        UpdatedOn = t.UpdatedOn.Value
      };
    });

    return await Task.WhenAll(dtoTasks);
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
}
