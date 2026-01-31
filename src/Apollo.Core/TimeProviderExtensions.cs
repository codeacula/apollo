namespace Apollo.Core;

/// <summary>
/// Extension methods for TimeProvider.
/// </summary>
public static class TimeProviderExtensions
{
  /// <summary>
  /// Gets the current UTC date and time as a DateTime.
  /// This is a convenience method for the common pattern timeProvider.GetUtcNow().DateTime.
  /// </summary>
  /// <param name="timeProvider">The time provider to get the current UTC time from.</param>
  /// <returns>The current UTC date and time as a DateTime.</returns>
  public static DateTime GetUtcDateTime(this TimeProvider timeProvider)
  {
    return timeProvider.GetUtcNow().DateTime;
  }
}
