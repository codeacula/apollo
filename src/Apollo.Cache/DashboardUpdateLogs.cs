using Microsoft.Extensions.Logging;

namespace Apollo.Cache;

public static partial class DashboardUpdateLogs
{
  [LoggerMessage(
    EventId = 3020,
    Level = LogLevel.Warning,
    Message = "Failed to publish dashboard overview update.")]
  public static partial void DashboardPublishFailed(ILogger logger, Exception exception);
}
