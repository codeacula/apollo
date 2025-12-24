using Apollo.Core;

using Microsoft.Extensions.Time.Testing;

namespace Apollo.Core.Tests;

public class TimeProviderExtensionsTests
{
  [Fact]
  public void GetUtcDateTimeReturnsCurrentUtcDateTime()
  {
    // Arrange
    var expectedTime = new DateTime(2024, 1, 15, 10, 30, 45, DateTimeKind.Utc);
    var fakeTimeProvider = new FakeTimeProvider(expectedTime);

    // Act
    var result = fakeTimeProvider.GetUtcDateTime();

    // Assert
    Assert.Equal(expectedTime.Year, result.Year);
    Assert.Equal(expectedTime.Month, result.Month);
    Assert.Equal(expectedTime.Day, result.Day);
    Assert.Equal(expectedTime.Hour, result.Hour);
    Assert.Equal(expectedTime.Minute, result.Minute);
    Assert.Equal(expectedTime.Second, result.Second);
  }

  [Fact]
  public void GetUtcDateTimeReturnsSameValueAsGetUtcNowDateTime()
  {
    // Arrange
    var expectedTime = new DateTime(2025, 12, 24, 12, 0, 0, DateTimeKind.Utc);
    var fakeTimeProvider = new FakeTimeProvider(expectedTime);

    // Act
    var extensionResult = fakeTimeProvider.GetUtcDateTime();
    var standardResult = fakeTimeProvider.GetUtcNow().DateTime;

    // Assert
    Assert.Equal(standardResult, extensionResult);
  }

  [Fact]
  public void GetUtcDateTimeWorksWithSystemTimeProvider()
  {
    // Arrange
    var systemTimeProvider = TimeProvider.System;
    var beforeCall = DateTime.UtcNow;

    // Act
    var result = systemTimeProvider.GetUtcDateTime();

    // Assert
    var afterCall = DateTime.UtcNow;
    Assert.InRange(result, beforeCall.AddSeconds(-1), afterCall.AddSeconds(1));
  }

  [Fact]
  public void GetUtcDateTimeReflectsTimeProviderChanges()
  {
    // Arrange
    var initialTime = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    var fakeTimeProvider = new FakeTimeProvider(initialTime);

    // Act
    var firstResult = fakeTimeProvider.GetUtcDateTime();
    fakeTimeProvider.Advance(TimeSpan.FromHours(5));
    var secondResult = fakeTimeProvider.GetUtcDateTime();

    // Assert
    Assert.Equal(initialTime, firstResult);
    Assert.Equal(initialTime.AddHours(5), secondResult);
    Assert.NotEqual(firstResult, secondResult);
  }
}
