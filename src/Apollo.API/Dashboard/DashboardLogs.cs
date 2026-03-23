using Microsoft.Extensions.Logging;

namespace Apollo.API.Dashboard;

public static partial class DashboardLogs
{
  [LoggerMessage(
    EventId = 5999,
    Level = LogLevel.Warning,
    Message = "Dashboard realtime broadcasting is unavailable because Redis could not be initialized.")]
  public static partial void DashboardBroadcastDisabled(ILogger logger, Exception exception);

  [LoggerMessage(
    EventId = 6000,
    Level = LogLevel.Warning,
    Message = "Failed to broadcast dashboard overview update.")]
  public static partial void DashboardBroadcastFailed(ILogger logger, Exception exception);
}
