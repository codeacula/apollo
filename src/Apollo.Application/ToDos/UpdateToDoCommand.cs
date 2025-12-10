using Apollo.Domain.ToDos.ValueObjects;

using FluentResults;

namespace Apollo.Application.ToDos;

public sealed record UpdateToDoCommand(
  ToDoId ToDoId,
  Description Description
) : IRequest<Result>;
