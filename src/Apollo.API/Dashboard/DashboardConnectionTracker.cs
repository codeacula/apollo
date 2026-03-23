namespace Apollo.API.Dashboard;

public sealed class DashboardConnectionTracker
{
  private int _connectionCount;

  public int ConnectionCount => _connectionCount;
  public bool HasConnections => _connectionCount > 0;

  public void Connected()
  {
    _ = Interlocked.Increment(ref _connectionCount);
  }

  public void Disconnected()
  {
    _ = Interlocked.Decrement(ref _connectionCount);

    if (_connectionCount < 0)
    {
      _connectionCount = 0;
    }
  }
}
