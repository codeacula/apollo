using Microsoft.Extensions.Logging;

namespace Apollo.Core.Logging;

/// <summary>
/// High-performance logging definitions for data access operations.
/// EventIds: 3200-3299
/// </summary>
public static partial class DataAccessLogs
{
  [LoggerMessage(
    EventId = 3200,
    Level = LogLevel.Warning,
    Message = "User not found or invalid username: {Username}")]
  public static partial void UserNotFound(ILogger logger, string username);

  [LoggerMessage(
    EventId = 3201,
    Level = LogLevel.Debug,
    Message = "Mock user access checked for {Username}: {HasAccess}")]
  public static partial void UserAccessChecked(ILogger logger, string username, bool hasAccess);
}
