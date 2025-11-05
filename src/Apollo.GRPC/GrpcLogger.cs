using Microsoft.Extensions.Logging;

namespace Apollo.GRPC;

public static partial class GrpcLogger
{
  [LoggerMessage(
      Level = LogLevel.Information,
      Message = "Starting call. Host: {Host} Type/Method: {Type} / {Method}")]
  public static partial void LogStartingCall(ILogger logger, string host, string type, string method);

  [LoggerMessage(
      Level = LogLevel.Information,
      Message = "Call succeeded. Host: {Host} Type/Method: {Type} / {Method}. {@Response}")]
  public static partial void LogCallSucceeded(ILogger logger, string host, string type, string method, object response);

  [LoggerMessage(
      Level = LogLevel.Error,
      Message = "Call failed. Host: {Host} Type/Method: {Type} / {Method}")]
  public static partial void LogCallFailed(ILogger logger, string host, string type, string method, Exception incException);
}
