using Apollo.Domain.People.ValueObjects;
using Apollo.Domain.ToDos.ValueObjects;

using FluentResults;

namespace Apollo.Application.ToDos.Commands;

public sealed record SetToDoEnergyCommand(
  PersonId PersonId,
  ToDoId ToDoId,
  Energy Energy
) : IRequest<Result>;
