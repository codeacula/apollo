using Apollo.Core.Logging;
using Apollo.Core.People;
using Apollo.Domain.People.ValueObjects;

using FluentResults;

using Microsoft.Extensions.Logging;

using StackExchange.Redis;

namespace Apollo.Cache;

public sealed class PersonCache(IConnectionMultiplexer redis, ILogger<PersonCache> logger) : IPersonCache
{
  private readonly IDatabase _db = redis.GetDatabase();
  private readonly ILogger<PersonCache> _logger = logger;
  private const string AccessKeyPrefix = "person:access:";
  private const string MappingKeyPrefix = "person:mapping:";
  private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

  public async Task<Result<PersonId?>> GetPersonIdAsync(PlatformId platformId)
  {
    try
    {
      var key = GetMappingCacheKey(platformId);
      var value = await _db.StringGetAsync(key);

      if (!value.HasValue)
      {
        CacheLogs.PlatformMappingCacheMiss(_logger, platformId.PlatformUserId, platformId.Platform);
        return Result.Ok<PersonId?>(null);
      }

      if (Guid.TryParse(value.ToString(), out var guid))
      {
        var personId = new PersonId(guid);
        CacheLogs.PlatformMappingCacheHit(_logger, platformId.PlatformUserId, platformId.Platform, personId.Value.ToString());
        return Result.Ok<PersonId?>(personId);
      }

      return Result.Fail<PersonId?>($"Failed to parse cached PersonId for user {platformId.PlatformUserId}");
    }
    catch (Exception ex)
    {
      CacheLogs.PlatformMappingCacheReadError(_logger, ex, platformId.PlatformUserId, platformId.Platform);
      return Result.Fail<PersonId?>($"Failed to read mapping from cache for user {platformId.PlatformUserId}: {ex.Message}");
    }
  }

  public async Task<Result> MapPlatformIdToPersonIdAsync(PlatformId platformId, PersonId personId)
  {
    try
    {
      var key = GetMappingCacheKey(platformId);
      _ = await _db.StringSetAsync(key, personId.Value.ToString(), CacheTtl);

      return Result.Ok();
    }
    catch (Exception ex)
    {
      return Result.Fail($"Failed to write mapping to cache for user {platformId.PlatformUserId}: {ex.Message}");
    }
  }

  public async Task<Result<bool?>> GetAccessAsync(PlatformId platformId)
  {
    try
    {
      var personIdResult = await GetPersonIdAsync(platformId);
      if (personIdResult.IsFailed)
      {
        return Result.Ok<bool?>(null);
      }

      if (!personIdResult.Value.HasValue)
      {
        CacheLogs.PersonCacheMiss(_logger, platformId.PlatformUserId, platformId.Platform);
        return Result.Ok<bool?>(null);
      }

      var personId = personIdResult.Value.Value;

      var key = GetAccessCacheKey(personId);
      var value = await _db.StringGetAsync(key);
      if (!value.HasValue)
      {
        return Result.Ok<bool?>(null);
      }
      var hasAccess = (bool)value;
      CacheLogs.PersonCacheHit(_logger, personId.Value.ToString(), hasAccess);
      return Result.Ok<bool?>(hasAccess);
    }
    catch (Exception ex)
    {
      CacheLogs.PersonCacheReadError(_logger, ex, platformId.PlatformUserId, platformId.Platform);
      return Result.Fail<bool?>($"Failed to read from cache for person {platformId.PlatformUserId} on {platformId.Platform}: {ex.Message}");
    }
  }

  public async Task<Result> SetAccessAsync(PersonId personId, bool hasAccess)
  {
    try
    {
      var key = GetAccessCacheKey(personId);
      _ = await _db.StringSetAsync(key, hasAccess, CacheTtl);

      CacheLogs.PersonCacheSet(_logger, personId.Value.ToString(), hasAccess);
      return Result.Ok();
    }
    catch (Exception ex)
    {
      CacheLogs.PersonCacheWriteError(_logger, ex, personId.Value.ToString());
      return Result.Fail($"Failed to write to cache for person {personId.Value}: {ex.Message}");
    }
  }

  public async Task<Result> InvalidateAccessAsync(PersonId personId)
  {
    try
    {
      var key = GetAccessCacheKey(personId);
      _ = await _db.KeyDeleteAsync(key);

      CacheLogs.PersonCacheInvalidated(_logger, personId.Value.ToString());
      return Result.Ok();
    }
    catch (Exception ex)
    {
      CacheLogs.PersonCacheDeleteError(_logger, ex, personId.Value.ToString());
      return Result.Fail($"Failed to invalidate cache for person {personId.Value}: {ex.Message}");
    }
  }

  public async Task<Result> InvalidatePlatformMappingAsync(PlatformId platformId)
  {
    try
    {
      var key = GetMappingCacheKey(platformId);
      _ = await _db.KeyDeleteAsync(key);

      CacheLogs.PlatformMappingCacheInvalidated(_logger, platformId.PlatformUserId, platformId.Platform);
      return Result.Ok();
    }
    catch (Exception ex)
    {
      CacheLogs.PlatformMappingCacheDeleteError(_logger, ex, platformId.PlatformUserId, platformId.Platform);
      return Result.Fail($"Failed to invalidate platform mapping cache for {platformId.PlatformUserId}: {ex.Message}");
    }
  }

  private static string GetMappingCacheKey(PlatformId platformId)
  {
    return MappingKeyPrefix + platformId.Platform + ":" + platformId.PlatformUserId;
  }

  private static string GetAccessCacheKey(PersonId personId)
  {
    return AccessKeyPrefix + personId.Value;
  }
}
