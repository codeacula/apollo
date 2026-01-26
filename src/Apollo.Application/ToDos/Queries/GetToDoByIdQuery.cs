using Apollo.Domain.ToDos.Models;
using Apollo.Domain.ToDos.ValueObjects;

using FluentResults;

namespace Apollo.Application.ToDos.Queries;

public sealed record GetToDoByIdQuery(ToDoId ToDoId) : IRequest<Result<ToDo>>;
