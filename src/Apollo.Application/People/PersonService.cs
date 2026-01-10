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
  public async Task<Result<Person>> GetOrCreateAsync(PlatformId platformId, CancellationToken cancellationToken = default)
  {
    var userResult = await personStore.GetByPlatformIdAsync(platformId, cancellationToken);

    if (userResult.IsSuccess)
    {
      return userResult;
    }

    var createResult = await personStore.CreateByPlatformIdAsync(platformId, cancellationToken);

    return createResult.IsSuccess
      ? createResult
      : Result.Fail<Person>($"Failed to get or create user {platformId.Username} on {platformId.Platform}: {createResult.GetErrorMessages()}");
  }

  public async Task<Result<HasAccess>> HasAccessAsync(PersonId personId, CancellationToken cancellationToken = default)
  {
    var cacheResult = await personCache.GetAccessAsync(personId);
    if (cacheResult.IsFailed)
    {
      ValidationLogs.CacheCheckFailed(logger, personId.Value.ToString(), cacheResult.GetErrorMessages());
      return Result.Fail<HasAccess>($"Cache error for user {personId.Value}: fail-closed policy denies access");
    }

    if (cacheResult.Value.HasValue)
    {
      var cachedAccess = cacheResult.Value.Value;
      ValidationLogs.CacheHit(logger, personId.Value.ToString(), cachedAccess);
      return Result.Ok(new HasAccess(cachedAccess));
    }

    // Default to returning true because the API will update cache if needed and reject the request
    return Result.Ok(new HasAccess(true));
  }

  public async Task<Result> MapPlatformIdToPersonIdAsync(PlatformId platformId, PersonId personId, CancellationToken cancellationToken = default)
  {
    var cacheResult = await personCache.MapPlatformIdToPersonIdAsync(platformId, personId);
    if (cacheResult.IsFailed)
    {
      ValidationLogs.PlatformIdMappingFailed(logger, platformId.PlatformUserId, platformId.Platform.ToString(), personId.Value.ToString(), cacheResult.GetErrorMessages());
      return Result.Fail($"Failed to map platform ID to person ID: {cacheResult.GetErrorMessages()}");
    }

    return Result.Ok();
  }
}
