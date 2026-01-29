using Apollo.Domain.People.ValueObjects;
using Apollo.Domain.ToDos.ValueObjects;

using FluentResults;

namespace Apollo.Application.ToDos.Commands;

public sealed record SetToDoInterestCommand(
  PersonId PersonId,
  ToDoId ToDoId,
  Interest Interest
) : IRequest<Result>;
