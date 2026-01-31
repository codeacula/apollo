using Apollo.Domain.People.ValueObjects;
using Apollo.Domain.ToDos.ValueObjects;

using FluentResults;

namespace Apollo.Application.ToDos.Commands;

public sealed record SetAllToDosAttributeCommand(
  PersonId PersonId,
  IReadOnlyList<ToDoId> ToDoIds,
  Priority? Priority = null,
  Energy? Energy = null,
  Interest? Interest = null
) : IRequest<Result<int>>;
