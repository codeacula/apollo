using Apollo.Domain.People.Models;
using Apollo.Domain.People.ValueObjects;

using FluentResults;

namespace Apollo.Application.People.Queries;

public sealed record GetOrCreatePersonByPlatformIdQuery(PlatformId PlatformId) : IRequest<Result<Person>>;
