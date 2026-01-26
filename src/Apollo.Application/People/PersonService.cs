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
  public async Task<Result<Person>> GetOrCreateAsync(PlatformId platformId, CancellationToken ct = default)
  {
    var userResult = await personStore.GetByPlatformIdAsync(platformId, ct);

    if (userResult.IsSuccess)
    {
      return userResult;
    }

    var createResult = await personStore.CreateByPlatformIdAsync(platformId, ct);

    if (createResult.IsFailed)
    {
      return Result.Fail<Person>($"Failed to get or create user {platformId.Username} on {platformId.Platform}: {createResult.GetErrorMessages()}");
    }

    // Best-effort: populate PlatformId -> PersonId cache mapping after successful creation.
    // Cache mapping failures are logged inside MapPlatformIdToPersonIdAsync and do not affect the result.
    _ = await personCache.MapPlatformIdToPersonIdAsync(platformId, createResult.Value.Id);

    return createResult;
  }

  public async Task<Result<HasAccess>> HasAccessAsync(PlatformId platformId)
  {
    var cacheResult = await personCache.GetAccessAsync(platformId);
    if (cacheResult.IsFailed)
    {
      ValidationLogs.CacheCheckFailed(logger, platformId.PlatformUserId, platformId.Platform, cacheResult.GetErrorMessages());
      return Result.Fail<HasAccess>($"Cache error for user {platformId.PlatformUserId}: fail-closed policy denies access");
    }

    if (cacheResult.Value.HasValue)
    {
      var cachedAccess = cacheResult.Value.Value;
      ValidationLogs.CacheHit(logger, platformId.PlatformUserId, platformId.Platform, cachedAccess);
      return Result.Ok(new HasAccess(cachedAccess));
    }

    // Default to returning true because the API will update cache if needed and reject the request
    return Result.Ok(new HasAccess(true));
  }

  public async Task<Result> MapPlatformIdToPersonIdAsync(PlatformId platformId, PersonId personId, CancellationToken ct = default)
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
