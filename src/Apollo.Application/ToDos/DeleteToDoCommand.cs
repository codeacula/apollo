using Apollo.Domain.ToDos.ValueObjects;

using FluentResults;

namespace Apollo.Application.ToDos;

public sealed record DeleteToDoCommand(ToDoId ToDoId) : IRequest<Result>;
