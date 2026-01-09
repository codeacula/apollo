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
  private const string KeyPrefix = "person:access:";
  private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

  public async Task<Result<bool?>> GetAccessAsync(PlatformId platformId)
  {
    try
    {
      var key = GetCacheKey(platformId);
      var value = await _db.StringGetAsync(key);

      if (!value.HasValue)
      {
        CacheLogs.CacheMiss(_logger, platformId.Value);
        return Result.Ok<bool?>(null);
      }

      var hasAccess = (bool)value;
      CacheLogs.CacheHit(_logger, platformId.Value, hasAccess);
      return Result.Ok<bool?>(hasAccess);
    }
    catch (Exception ex)
    {
      CacheLogs.CacheReadError(_logger, ex, platformId.Value);
      return Result.Fail<bool?>($"Failed to read from cache for user {platformId.Value}: {ex.Message}");
    }
  }

  public async Task<Result> SetAccessAsync(PersonId personId, bool hasAccess)
  {
    try
    {
      var key = GetCacheKey(personId);
      _ = await _db.StringSetAsync(key, hasAccess, CacheTtl);

      CacheLogs.CacheSet(_logger, personId.Value, hasAccess);
      return Result.Ok();
    }
    catch (Exception ex)
    {
      CacheLogs.CacheWriteError(_logger, ex, personId.Value);
      return Result.Fail($"Failed to write to cache for user {personId.Value}: {ex.Message}");
    }
  }

  public async Task<Result> InvalidateAccessAsync(PersonId personId)
  {
    try
    {
      var key = GetCacheKey(personId);
      _ = await _db.KeyDeleteAsync(key);

      CacheLogs.CacheInvalidated(_logger, personId.Value);
      return Result.Ok();
    }
    catch (Exception ex)
    {
      CacheLogs.CacheDeleteError(_logger, ex, personId.Value);
      return Result.Fail($"Failed to invalidate cache for user {personId.Value}: {ex.Message}");
    }
  }

  private static string GetCacheKey(PlatformId platformId)
  {
    return KeyPrefix + platformId.Platform + ":" + platformId.PlatformUserId;
  }
}
