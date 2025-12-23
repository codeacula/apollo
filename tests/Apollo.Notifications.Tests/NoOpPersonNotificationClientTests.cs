using Apollo.Core.Notifications;
using Apollo.Domain.People.Models;
using Apollo.Domain.People.ValueObjects;

using FluentAssertions;

namespace Apollo.Notifications.Tests;

public class NoOpPersonNotificationClientTests
{
  [Fact]
  public async Task SendNotificationAsyncReturnsSuccess()
  {
    // Arrange
    var client = new NoOpPersonNotificationClient();
    var person = new Person
    {
      Id = new PersonId(Guid.NewGuid()),
      Username = new Username("testuser", Domain.Common.Enums.Platform.Discord),
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
