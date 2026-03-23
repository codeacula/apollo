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
    var result = Interlocked.Decrement(ref _connectionCount);

    if (result < 0)
    {
      Interlocked.CompareExchange(ref _connectionCount, 0, result);
    }
  }
}
