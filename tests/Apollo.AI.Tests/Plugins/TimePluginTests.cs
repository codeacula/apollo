using Apollo.AI.Plugins;

namespace Apollo.AI.Tests.Plugins;

public class TimePluginTests
{
  private readonly TimeProvider _mockTimeProvider;
  private readonly TimePlugin _plugin;
  private readonly DateTimeOffset _fixedNow = new(2025, 12, 12, 14, 30, 0, TimeSpan.Zero);

  public TimePluginTests()
  {
    _mockTimeProvider = new FixedTimeProvider(_fixedNow);
    _plugin = new TimePlugin(_mockTimeProvider);
  }

  [Fact]
  public void GetTimeReturnsCurrentTimeInIsoFormat()
  {
    var result = _plugin.GetTime();
    Assert.Equal("14:30:00", result);
  }

  [Fact]
  public void GetDateTimeReturnsCurrentDateTimeInIsoFormat()
  {
    var result = _plugin.GetDateTime();
    Assert.Equal("2025-12-12T14:30:00", result);
  }
  private sealed class FixedTimeProvider(DateTimeOffset fixedTime) : TimeProvider
  {
    private readonly DateTimeOffset _fixedTime = fixedTime;

    public override DateTimeOffset GetUtcNow() => _fixedTime;
  }
}
