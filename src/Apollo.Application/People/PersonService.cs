using Apollo.Core;
using Apollo.Core.Logging;
using Apollo.Core.People;
using Apollo.Domain.People.Models;
using Apollo.Domain.People.ValueObjects;

using FluentResults;

namespace Apollo.Application.People;

public sealed class PersonService(
  IPersonStore personStore,
  IPersonCache personCache,
  ILogger<PersonService> logger) : IPersonService
{
  public async Task<Result<Person>> GetOrCreateAsync(PersonId personId, Username username, CancellationToken cancellationToken = default)
  {
    if (!username.IsValid)
    {
      ValidationLogs.InvalidUsername(logger);
      return Result.Fail<Person>("Invalid username");
    }

    if (!personId.IsValid)
    {
      ValidationLogs.InvalidPersonId(logger);
      return Result.Fail<Person>("Invalid person id");
    }

    var userResult = await personStore.GetAsync(personId, cancellationToken);

    if (userResult.IsSuccess)
    {
      return userResult;
    }

    var createResult = await personStore.CreateAsync(personId, username, cancellationToken);

    return createResult.IsSuccess ? createResult : Result.Fail<Person>($"Failed to get or create user {username}");
  }

  public async Task<Result> GrantAccessAsync(PersonId personId, CancellationToken cancellationToken = default)
  {
    if (!personId.IsValid)
    {
      ValidationLogs.InvalidPersonId(logger);
      return Result.Fail("Invalid person id");
    }

    var userResult = await personStore.GetAsync(personId, cancellationToken);

    return userResult.IsFailed
      ? Result.Fail($"User {personId.Value} not found")
      : await personStore.GrantAccessAsync(userResult.Value.Id, cancellationToken);
  }

  public async Task<Result<HasAccess>> HasAccessAsync(PersonId personId, CancellationToken cancellationToken = default)
  {
    if (!personId.IsValid)
    {
      ValidationLogs.InvalidPersonId(logger);
      return Result.Fail<HasAccess>("Invalid person id");
    }

    var cacheResult = await personCache.GetAccessAsync(personId);
    if (cacheResult.IsFailed)
    {
      ValidationLogs.CacheCheckFailed(logger, personId.Value, cacheResult.GetErrorMessages());
      return Result.Fail<HasAccess>($"Cache error for user {personId.Value}: fail-closed policy denies access");
    }

    if (cacheResult.Value.HasValue)
    {
      var cachedAccess = cacheResult.Value.Value;
      ValidationLogs.CacheHit(logger, personId.Value, cachedAccess);
      return Result.Ok(new HasAccess(cachedAccess));
    }

    // Default to returning true because the API will update cache if needed and reject the request
    return Result.Ok(new HasAccess(true));
  }
}
