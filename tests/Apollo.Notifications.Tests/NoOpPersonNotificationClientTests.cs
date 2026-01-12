using Apollo.Core.Notifications;
using Apollo.Domain.Common.Enums;
using Apollo.Domain.People.Models;
using Apollo.Domain.People.ValueObjects;

using FluentAssertions;

namespace Apollo.Notifications.Tests;

public class NoOpPersonNotificationClientTests
{
  [Fact]
  public async Task SendNotificationAsyncReturnsSuccessAsync()
  {
    // Arrange
    var client = new NoOpPersonNotificationClient();
    var person = new Person
    {
      Id = new PersonId(Guid.NewGuid()),
      PlatformId = new PlatformId("testuser", "123", Platform.Discord),
      Username = new Username("testuser"),
      HasAccess = new HasAccess(true),
      NotificationChannels = [],
      CreatedOn = new Domain.Common.ValueObjects.CreatedOn(DateTime.UtcNow),
      UpdatedOn = new Domain.Common.ValueObjects.UpdatedOn(DateTime.UtcNow)
    };
    var notification = new Notification { Content = "Test message" };

    // Act
    var result = await client.SendNotificationAsync(person, notification);

    // Assert
    _ = result.IsSuccess.Should().BeTrue();
  }
}
