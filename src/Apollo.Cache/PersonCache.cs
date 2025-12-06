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

  public async Task<Result<bool?>> GetAccessAsync(Username username)
  {
    try
    {
      var key = GetCacheKey(username);
      var value = await _db.StringGetAsync(key);

      if (!value.HasValue)
      {
        CacheLogs.CacheMiss(_logger, username.Value);
        return Result.Ok<bool?>(null);
      }

      var hasAccess = (bool)value;
      CacheLogs.CacheHit(_logger, username.Value, hasAccess);
      return Result.Ok<bool?>(hasAccess);
    }
    catch (Exception ex)
    {
      CacheLogs.CacheReadError(_logger, ex, username.Value);
      return Result.Fail<bool?>($"Failed to read from cache for user {username.Value}: {ex.Message}");
    }
  }

  public async Task<Result> SetAccessAsync(Username username, bool hasAccess)
  {
    try
    {
      var key = GetCacheKey(username);
      _ = await _db.StringSetAsync(key, hasAccess, CacheTtl);

      CacheLogs.CacheSet(_logger, username.Value, hasAccess);
      return Result.Ok();
    }
    catch (Exception ex)
    {
      CacheLogs.CacheWriteError(_logger, ex, username.Value);
      return Result.Fail($"Failed to write to cache for user {username.Value}: {ex.Message}");
    }
  }

  public async Task<Result> InvalidateAccessAsync(Username username)
  {
    try
    {
      var key = GetCacheKey(username);
      _ = await _db.KeyDeleteAsync(key);

      CacheLogs.CacheInvalidated(_logger, username.Value);
      return Result.Ok();
    }
    catch (Exception ex)
    {
      CacheLogs.CacheDeleteError(_logger, ex, username.Value);
      return Result.Fail($"Failed to invalidate cache for user {username.Value}: {ex.Message}");
    }
  }

  private static string GetCacheKey(Username username)
  {
    return KeyPrefix + username.Value.ToLowerInvariant() + ":" + username.Platform.ToString().ToLowerInvariant();
  }
}
