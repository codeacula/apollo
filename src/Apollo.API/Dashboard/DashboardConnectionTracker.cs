namespace Apollo.API.Dashboard;

public sealed class DashboardConnectionTracker
{
  private int _connectionCount;

  public int ConnectionCount => Volatile.Read(ref _connectionCount);
  public bool HasConnections => Volatile.Read(ref _connectionCount) > 0;

  public void Connected()
  {
    _ = Interlocked.Increment(ref _connectionCount);
  }

  public void Disconnected()
  {
    var result = Interlocked.Decrement(ref _connectionCount);

    if (result >= 0)
    {
      return;
    }

    // CAS retry loop: spin until we successfully clamp to 0 from a negative value.
    int current;
    do
    {
      current = _connectionCount;
      if (current >= 0)
      {
        break;
      }
    } while (Interlocked.CompareExchange(ref _connectionCount, 0, current) != current);
  }
}
