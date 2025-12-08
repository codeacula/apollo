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
  public async Task<Result<Person>> GetOrCreateAsync(Username username, CancellationToken cancellationToken = default)
  {
    if (!username.IsValid)
    {
      ValidationLogs.InvalidUsername(logger);
      return Result.Fail<Person>("Invalid username");
    }

    var userResult = await personStore.GetByUsernameAsync(username, cancellationToken);

    if (userResult.IsSuccess)
    {
      return userResult;
    }

    var createResult = await personStore.CreateAsync(new(Guid.NewGuid()), username, cancellationToken);

    return createResult.IsSuccess ? createResult : Result.Fail<Person>($"Failed to get or create user {username}");
  }

  public async Task<Result> GrantAccessAsync(Username username, CancellationToken cancellationToken = default)
  {
    if (!username.IsValid)
    {
      ValidationLogs.InvalidUsername(logger);
      return Result.Fail("Invalid username");
    }

    var userResult = await personStore.GetByUsernameAsync(username, cancellationToken);

    return userResult.IsFailed
      ? Result.Fail($"User {username} not found")
      : await personStore.GrantAccessAsync(userResult.Value.Id, cancellationToken);
  }

  public async Task<Result<HasAccess>> HasAccessAsync(Username username, CancellationToken cancellationToken = default)
  {
    if (!username.IsValid)
    {
      ValidationLogs.InvalidUsername(logger);
      return Result.Fail<HasAccess>("Invalid username");
    }

    var cacheResult = await personCache.GetAccessAsync(username);
    if (cacheResult.IsFailed)
    {
      ValidationLogs.CacheCheckFailed(logger, username, string.Join(", ", cacheResult.Errors.Select(e => e.Message)));
      return Result.Fail<HasAccess>($"Cache error for user {username}: fail-closed policy denies access");
    }

    if (cacheResult.Value.HasValue)
    {
      var cachedAccess = cacheResult.Value.Value;
      ValidationLogs.CacheHit(logger, username, cachedAccess);
      return Result.Ok(new HasAccess(cachedAccess));
    }

    // Default to returning true because the API will update cache if needed and reject the request
    return Result.Ok(new HasAccess(true));
  }
}
