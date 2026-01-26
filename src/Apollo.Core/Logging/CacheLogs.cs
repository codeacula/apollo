using Apollo.Domain.Common.Enums;

using Microsoft.Extensions.Logging;

namespace Apollo.Core.Logging;

/// <summary>
/// High-performance logging definitions for cache operations.
/// EventIds: 3000-3099
/// </summary>
public static partial class CacheLogs
{
  [LoggerMessage(
    EventId = 3000,
    Level = LogLevel.Debug,
    Message = "Cache miss for: {PlatformUserId}, platform id: {Platform}")]
  public static partial void CacheMiss(ILogger logger, string platformUserId, Platform platform);

  [LoggerMessage(
    EventId = 3001,
    Level = LogLevel.Debug,
    Message = "Cache hit: {PlatformUserId}, platform id: {Platform}, access: {HasAccess}")]
  public static partial void CacheHit(ILogger logger, string platformUserId, Platform platform, bool hasAccess);

  [LoggerMessage(
    EventId = 3002,
    Level = LogLevel.Debug,
    Message = "Cache set: {PlatformUserId}, platform id: {Platform}, access: {HasAccess}")]
  public static partial void CacheSet(ILogger logger, string platformUserId, Platform platform, bool hasAccess);

  [LoggerMessage(
    EventId = 3003,
    Level = LogLevel.Information,
    Message = "Cache invalidated for: {PlatformUserId}, platform id: {Platform}")]
  public static partial void CacheInvalidated(ILogger logger, string platformUserId, Platform platform);

  [LoggerMessage(
    EventId = 3004,
    Level = LogLevel.Error,
    Message = "Error reading from cache for: {PlatformUserId}, platform id: {Platform}")]
  public static partial void CacheReadError(ILogger logger, Exception exception, string platformUserId, Platform platform);

  [LoggerMessage(
    EventId = 3005,
    Level = LogLevel.Error,
    Message = "Error writing to cache for: {PlatformUserId}, platform id: {Platform}")]
  public static partial void CacheWriteError(ILogger logger, Exception exception, string platformUserId, Platform platform);

  [LoggerMessage(
    EventId = 3006,
    Level = LogLevel.Error,
    Message = "Error deleting from cache for: {PlatformUserId}, platform id: {Platform}")]
  public static partial void CacheDeleteError(ILogger logger, Exception exception, string platformUserId, Platform platform);

  [LoggerMessage(
    EventId = 3007,
    Level = LogLevel.Error,
    Message = "Unable to set value to cache: {ErrorMessage}")]
  public static partial void UnableToSetToCache(ILogger logger, string errorMessage);

  [LoggerMessage(
    EventId = 3010,
    Level = LogLevel.Debug,
    Message = "Cache miss for person id: {PersonPlatformId} on {Platform}")]
  public static partial void PersonCacheMiss(ILogger logger, string personPlatformId, Platform platform);

  [LoggerMessage(
    EventId = 3011,
    Level = LogLevel.Debug,
    Message = "Cache hit for person id: {PersonId}, access: {HasAccess}")]
  public static partial void PersonCacheHit(ILogger logger, string personId, bool hasAccess);

  [LoggerMessage(
    EventId = 3012,
    Level = LogLevel.Debug,
    Message = "Cache set for person id: {PersonId}, access: {HasAccess}")]
  public static partial void PersonCacheSet(ILogger logger, string personId, bool hasAccess);

  [LoggerMessage(
    EventId = 3013,
    Level = LogLevel.Information,
    Message = "Cache invalidated for person id: {PersonId}")]
  public static partial void PersonCacheInvalidated(ILogger logger, string personId);

  [LoggerMessage(
    EventId = 3014,
    Level = LogLevel.Error,
    Message = "Error reading from cache for person id: {PersonPlatformId} on {Platform}")]
  public static partial void PersonCacheReadError(ILogger logger, Exception exception, string personPlatformId, Platform platform);

  [LoggerMessage(
    EventId = 3015,
    Level = LogLevel.Error,
    Message = "Error writing to cache for person id: {PersonId}")]
  public static partial void PersonCacheWriteError(ILogger logger, Exception exception, string personId);

  [LoggerMessage(
    EventId = 3016,
    Level = LogLevel.Error,
    Message = "Error deleting from cache for person id: {PersonId}")]
  public static partial void PersonCacheDeleteError(ILogger logger, Exception exception, string personId);

  [LoggerMessage(
    EventId = 3017,
    Level = LogLevel.Debug,
    Message = "Platform mapping cache miss for: {PlatformUserId}, platform: {Platform}")]
  public static partial void PlatformMappingCacheMiss(ILogger logger, string platformUserId, Platform platform);

  [LoggerMessage(
    EventId = 3018,
    Level = LogLevel.Debug,
    Message = "Platform mapping cache hit for: {PlatformUserId}, platform: {Platform}, person id: {PersonId}")]
  public static partial void PlatformMappingCacheHit(ILogger logger, string platformUserId, Platform platform, string personId);

  [LoggerMessage(
    EventId = 3019,
    Level = LogLevel.Error,
    Message = "Error reading platform mapping from cache for: {PlatformUserId}, platform: {Platform}")]
  public static partial void PlatformMappingCacheReadError(ILogger logger, Exception exception, string platformUserId, Platform platform);

  [LoggerMessage(
    EventId = 3020,
    Level = LogLevel.Information,
    Message = "Platform mapping cache invalidated for: {PlatformUserId}, platform: {Platform}")]
  public static partial void PlatformMappingCacheInvalidated(ILogger logger, string platformUserId, Platform platform);

  [LoggerMessage(
    EventId = 3021,
    Level = LogLevel.Error,
    Message = "Error deleting platform mapping from cache for: {PlatformUserId}, platform: {Platform}")]
  public static partial void PlatformMappingCacheDeleteError(ILogger logger, Exception exception, string platformUserId, Platform platform);
}
