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
    Message = "Cache miss for user: {Username}")]
  public static partial void CacheMiss(ILogger logger, string username);

  [LoggerMessage(
    EventId = 3001,
    Level = LogLevel.Debug,
    Message = "Cache hit for user {Username}: {HasAccess}")]
  public static partial void CacheHit(ILogger logger, string username, bool hasAccess);

  [LoggerMessage(
    EventId = 3002,
    Level = LogLevel.Debug,
    Message = "Cache set for user {Username}: {HasAccess}")]
  public static partial void CacheSet(ILogger logger, string username, bool hasAccess);

  [LoggerMessage(
    EventId = 3003,
    Level = LogLevel.Information,
    Message = "Cache invalidated for user: {Username}")]
  public static partial void CacheInvalidated(ILogger logger, string username);

  [LoggerMessage(
    EventId = 3004,
    Level = LogLevel.Error,
    Message = "Error reading from cache for user: {Username}")]
  public static partial void CacheReadError(ILogger logger, Exception exception, string username);

  [LoggerMessage(
    EventId = 3005,
    Level = LogLevel.Error,
    Message = "Error writing to cache for user: {Username}")]
  public static partial void CacheWriteError(ILogger logger, Exception exception, string username);

  [LoggerMessage(
    EventId = 3006,
    Level = LogLevel.Error,
    Message = "Error deleting from cache for user: {Username}")]
  public static partial void CacheDeleteError(ILogger logger, Exception exception, string username);
}
