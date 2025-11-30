using Microsoft.Extensions.Logging;

namespace Apollo.Core.Logging;

/// <summary>
/// High-performance logging definitions for gRPC operations.
/// EventIds: 1000-1099
/// </summary>
public static partial class GrpcLogs
{
  [LoggerMessage(
      EventId = 1000,
      Level = LogLevel.Information,
      Message = "Starting call. Host: {Host} Type/Method: {Type} / {Method}")]
  public static partial void LogStartingCall(ILogger logger, string host, string type, string method);

  [LoggerMessage(
      EventId = 1001,
      Level = LogLevel.Information,
      Message = "Call succeeded. Host: {Host} Type/Method: {Type} / {Method}. {@Response}")]
  public static partial void LogCallSucceeded(ILogger logger, string host, string type, string method, object response);

  [LoggerMessage(
      EventId = 1002,
      Level = LogLevel.Error,
      Message = "Call failed. Host: {Host} Type/Method: {Type} / {Method}")]
  public static partial void LogCallFailed(ILogger logger, string host, string type, string method, Exception incException);

  [LoggerMessage(
      EventId = 1003,
      Level = LogLevel.Error,
      Message = "Server exception occurred. Host: {Host} Type/Method: {Method}")]
  public static partial void LogServerException(ILogger logger, string host, string method, Exception exception);
}
