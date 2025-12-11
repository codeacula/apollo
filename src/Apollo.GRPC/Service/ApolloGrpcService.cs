using Apollo.Application.Conversations;
using Apollo.Application.ToDos;
using Apollo.Core.Conversations;
using Apollo.Domain.People.ValueObjects;
using Apollo.Domain.ToDos.ValueObjects;
using Apollo.GRPC.Contracts;

using MediatR;

namespace Apollo.GRPC.Service;

public sealed class ApolloGrpcService(IMediator mediator) : IApolloGrpcService
{
  public async Task<GrpcResult<string>> SendApolloMessageAsync(NewMessage message)
  {
    var requestResult = await mediator.Send(new ProcessIncomingMessageCommand(message));
    return requestResult.IsSuccess ?
      requestResult.Value.Content.Value :
      requestResult.Errors.Select(e => new GrpcError(e.Message)).ToArray();
  }

  public async Task<GrpcResult<ToDoDto>> CreateToDoAsync(CreateToDoRequest request)
  {
    var command = new CreateToDoCommand(
      new PersonId(request.PersonId),
      new Description(request.Description),
      request.ReminderDate
    );

    var result = await mediator.Send(command);

    if (result.IsFailed)
    {
      return result.Errors.Select(e => new GrpcError(e.Message)).ToArray();
    }

    var todo = result.Value;
    return new ToDoDto
    {
      Id = todo.Id.Value,
      PersonId = todo.PersonId.Value,
      Description = todo.Description.Value,
      CreatedOn = todo.CreatedOn.Value,
      UpdatedOn = todo.UpdatedOn.Value
    };
  }

  public async Task<GrpcResult<ToDoDto>> GetToDoAsync(GetToDoRequest request)
  {
    var query = new GetToDoByIdQuery(new ToDoId(request.ToDoId));
    var result = await mediator.Send(query);

    if (result.IsFailed)
    {
      return result.Errors.Select(e => new GrpcError(e.Message)).ToArray();
    }

    var todo = result.Value;
    return new ToDoDto
    {
      Id = todo.Id.Value,
      PersonId = todo.PersonId.Value,
      Description = todo.Description.Value,
      CreatedOn = todo.CreatedOn.Value,
      UpdatedOn = todo.UpdatedOn.Value
    };
  }

  public async Task<GrpcResult<ToDoDto[]>> GetPersonToDosAsync(GetPersonToDosRequest request)
  {
    var query = new GetToDosByPersonIdQuery(new PersonId(request.PersonId));
    var result = await mediator.Send(query);

    return result.IsFailed
      ? (GrpcResult<ToDoDto[]>)result.Errors.Select(e => new GrpcError(e.Message)).ToArray()
      : (GrpcResult<ToDoDto[]>)result.Value.Select(t => new ToDoDto
      {
        Id = t.Id.Value,
        PersonId = t.PersonId.Value,
        Description = t.Description.Value,
        CreatedOn = t.CreatedOn.Value,
        UpdatedOn = t.UpdatedOn.Value
      }).ToArray();
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
