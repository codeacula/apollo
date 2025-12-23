using Apollo.Domain.People.ValueObjects;
using Apollo.Domain.ToDos.Models;

using FluentResults;

namespace Apollo.Application.ToDos.Queries;

public sealed record GetToDosByPersonIdQuery(PersonId PersonId) : IRequest<Result<IEnumerable<ToDo>>>;
