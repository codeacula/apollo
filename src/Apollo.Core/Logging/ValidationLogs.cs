using Microsoft.Extensions.Logging;

namespace Apollo.Core.Logging;

/// <summary>
/// High-performance logging definitions for user validation operations.
/// EventIds: 3100-3199
/// </summary>
public static partial class ValidationLogs
{
  [LoggerMessage(
    EventId = 3100,
    Level = LogLevel.Warning,
    Message = "Invalid username provided for validation")]
  public static partial void InvalidUsername(ILogger logger);

  [LoggerMessage(
    EventId = 3101,
    Level = LogLevel.Debug,
    Message = "Cache hit for user {Username}: {HasAccess}")]
  public static partial void CacheHit(ILogger logger, string username, bool hasAccess);

  [LoggerMessage(
    EventId = 3102,
    Level = LogLevel.Debug,
    Message = "Cache miss for user: {Username}")]
  public static partial void CacheMiss(ILogger logger, string username);

  [LoggerMessage(
    EventId = 3103,
    Level = LogLevel.Error,
    Message = "Cache check failed for user {Username}: {Errors}")]
  public static partial void CacheCheckFailed(ILogger logger, string username, string errors);

  [LoggerMessage(
    EventId = 3104,
    Level = LogLevel.Error,
    Message = "Data access failed for user {Username}: {Errors}")]
  public static partial void DataAccessFailed(ILogger logger, string username, string errors);

  [LoggerMessage(
    EventId = 3105,
    Level = LogLevel.Warning,
    Message = "Cache update failed for user {Username}: {Errors}")]
  public static partial void CacheUpdateFailed(ILogger logger, string username, string errors);

  [LoggerMessage(
    EventId = 3106,
    Level = LogLevel.Debug,
    Message = "Cache updated for user {Username}: {HasAccess}")]
  public static partial void CacheUpdated(ILogger logger, string username, bool hasAccess);

  [LoggerMessage(
    EventId = 3107,
    Level = LogLevel.Information,
    Message = "User {Username} validated: {HasAccess}")]
  public static partial void UserValidated(ILogger logger, string username, bool hasAccess);

  [LoggerMessage(
    EventId = 3108,
    Level = LogLevel.Warning,
    Message = "User validation failed for {Username}: {Errors}")]
  public static partial void ValidationFailed(ILogger logger, string username, string errors);

  [LoggerMessage(
    EventId = 3109,
    Level = LogLevel.Information,
    Message = "Access denied for user: {Username}")]
  public static partial void AccessDenied(ILogger logger, string username);
}
