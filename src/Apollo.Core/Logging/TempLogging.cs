using Microsoft.Extensions.Logging;

namespace Apollo.Core.Logging;

public static partial class TempLogging
{
  [LoggerMessage(
      EventId = 9000,
      Level = LogLevel.Information,
      Message = "Incoming message from user {Username}: {Message}.")]
  public static partial void SendingMessage(ILogger logger, string username, string message);

}
