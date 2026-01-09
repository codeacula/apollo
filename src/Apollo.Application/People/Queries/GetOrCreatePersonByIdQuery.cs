using Apollo.Domain.People.Models;
using Apollo.Domain.People.ValueObjects;

using FluentResults;

namespace Apollo.Application.People.Queries;

public sealed record GetOrCreatePersonByIdQuery(PersonId PersonId, Username Username) : IRequest<Result<Person>>;
