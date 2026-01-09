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
    Message = "Cache miss for person id: {PersonId}")]
  public static partial void CacheMiss(ILogger logger, string personId);

  [LoggerMessage(
    EventId = 3001,
    Level = LogLevel.Debug,
    Message = "Cache hit for person id {PersonId}: {HasAccess}")]
  public static partial void CacheHit(ILogger logger, string personId, bool hasAccess);

  [LoggerMessage(
    EventId = 3002,
    Level = LogLevel.Debug,
    Message = "Cache set for person id {PersonId}: {HasAccess}")]
  public static partial void CacheSet(ILogger logger, string personId, bool hasAccess);

  [LoggerMessage(
    EventId = 3003,
    Level = LogLevel.Information,
    Message = "Cache invalidated for person id: {PersonId}")]
  public static partial void CacheInvalidated(ILogger logger, string personId);

  [LoggerMessage(
    EventId = 3004,
    Level = LogLevel.Error,
    Message = "Error reading from cache for person id: {PersonId}")]
  public static partial void CacheReadError(ILogger logger, Exception exception, string personId);

  [LoggerMessage(
    EventId = 3005,
    Level = LogLevel.Error,
    Message = "Error writing to cache for person id: {PersonId}")]
  public static partial void CacheWriteError(ILogger logger, Exception exception, string personId);

  [LoggerMessage(
    EventId = 3006,
    Level = LogLevel.Error,
    Message = "Error deleting from cache for person id: {PersonId}")]
  public static partial void CacheDeleteError(ILogger logger, Exception exception, string personId);

  [LoggerMessage(
    EventId = 3007,
    Level = LogLevel.Error,
    Message = "Unable to set value to cache: {ErrorMessages}")]
  public static partial void UnableToSetToCache(ILogger logger, string[] errorMessages);
}
