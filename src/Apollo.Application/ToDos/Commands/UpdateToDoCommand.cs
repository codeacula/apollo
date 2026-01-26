using Apollo.Domain.ToDos.ValueObjects;

using FluentResults;

namespace Apollo.Application.ToDos.Commands;

public sealed record UpdateToDoCommand(
  ToDoId ToDoId,
  Description Description
) : IRequest<Result>;
