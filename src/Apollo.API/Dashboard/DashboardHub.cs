using Microsoft.AspNetCore.SignalR;

namespace Apollo.API.Dashboard;

public sealed class DashboardHub(DashboardConnectionTracker connectionTracker) : Hub
{
  public override Task OnConnectedAsync()
  {
    connectionTracker.Connected();
    return base.OnConnectedAsync();
  }

  public override Task OnDisconnectedAsync(Exception? exception)
  {
    connectionTracker.Disconnected();
    return base.OnDisconnectedAsync(exception);
  }
}
