using Apollo.Domain.People.ValueObjects;
using Apollo.Domain.ToDos.ValueObjects;

using FluentResults;

namespace Apollo.Application.ToDos.Commands;

public sealed record SetToDoPriorityCommand(
  PersonId PersonId,
  ToDoId ToDoId,
  Priority Priority
) : IRequest<Result>;
