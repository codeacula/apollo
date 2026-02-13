using Apollo.Domain.Common.Enums;

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
    EventId = 3110,
    Level = LogLevel.Warning,
    Message = "Invalid person id provided for validation")]
  public static partial void InvalidPersonId(ILogger logger);

  [LoggerMessage(
    EventId = 3101,
    Level = LogLevel.Debug,
    Message = "Cache hit for person id {PersonPlatformId} on {Platform}: {HasAccess}")]
  public static partial void CacheHit(ILogger logger, string personPlatformId, Platform platform, bool hasAccess);

  [LoggerMessage(
    EventId = 3102,
    Level = LogLevel.Debug,
    Message = "Cache miss for person id: {PersonId}")]
  public static partial void CacheMiss(ILogger logger, string personId);

  [LoggerMessage(
    EventId = 3103,
    Level = LogLevel.Error,
    Message = "Cache check failed for person id {PersonPlatformId} on {Platform}: {Errors}")]
  public static partial void CacheCheckFailed(ILogger logger, string personPlatformId, Platform platform, string errors);

  [LoggerMessage(
    EventId = 3104,
    Level = LogLevel.Error,
    Message = "Data access failed for person id {PersonId}: {Errors}")]
  public static partial void DataAccessFailed(ILogger logger, string personId, string errors);

  [LoggerMessage(
    EventId = 3105,
    Level = LogLevel.Warning,
    Message = "Cache update failed for person id {PersonId}: {Errors}")]
  public static partial void CacheUpdateFailed(ILogger logger, string personId, string errors);

  [LoggerMessage(
    EventId = 3106,
    Level = LogLevel.Debug,
    Message = "Cache updated for person id {PersonId}: {HasAccess}")]
  public static partial void CacheUpdated(ILogger logger, string personId, bool hasAccess);

  [LoggerMessage(
    EventId = 3107,
    Level = LogLevel.Information,
    Message = "Person {PersonId} validated: {HasAccess}")]
  public static partial void UserValidated(ILogger logger, string personId, bool hasAccess);

  [LoggerMessage(
    EventId = 3108,
    Level = LogLevel.Warning,
    Message = "Person validation failed for {PersonId}: {Errors}")]
  public static partial void ValidationFailed(ILogger logger, string personId, string errors);

  [LoggerMessage(
    EventId = 3109,
    Level = LogLevel.Information,
    Message = "Access denied for person id: {PersonId}")]
  public static partial void AccessDenied(ILogger logger, string personId);

  [LoggerMessage(
    EventId = 3111,
    Level = LogLevel.Warning,
    Message = "Failed to map PlatformId {PlatformUserId} on {Platform} to PersonId {PersonId}: {Errors}")]
  public static partial void PlatformIdMappingFailed(ILogger logger, string platformUserId, string platform, string personId, string errors);
}
