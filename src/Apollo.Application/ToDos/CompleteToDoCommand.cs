using Apollo.Domain.ToDos.ValueObjects;

using FluentResults;

namespace Apollo.Application.ToDos;

public sealed record CompleteToDoCommand(ToDoId ToDoId) : IRequest<Result>;
