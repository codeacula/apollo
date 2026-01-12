using Apollo.Core.Notifications;
using Apollo.Domain.Common.Enums;
using Apollo.Domain.People.Models;
using Apollo.Domain.People.ValueObjects;

using FluentAssertions;

using FluentResults;

using NSubstitute;

namespace Apollo.Notifications.Tests;

public class PersonNotificationClientTests
{
  [Fact]
  public async Task SendNotificationAsyncNoEnabledChannelsReturnsFailureAsync()
  {
    // Arrange
    var channels = Enumerable.Empty<INotificationChannel>();
    var client = new PersonNotificationClient(channels);

    var notification = new Notification { Content = "Test message" };

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

    // Act
    var result = await client.SendNotificationAsync(person, notification);

    // Assert
    _ = result.IsFailed.Should().BeTrue();
    _ = result.Errors.Should().Contain(e => e.Message.Contains("no enabled notification channels"));
  }

  [Fact]
  public async Task SendNotificationAsyncSucceedsWithEnabledChannelAsync()
  {
    // Arrange
    var mockChannel = Substitute.For<INotificationChannel>();
    _ = mockChannel.ChannelType.Returns(NotificationChannelType.Discord);
    _ = mockChannel.SendAsync(Arg.Any<string>(), Arg.Any<Notification>(), Arg.Any<CancellationToken>())
      .Returns(Result.Ok());

    var channels = new[] { mockChannel };
    var client = new PersonNotificationClient(channels);

    var notification = new Notification { Content = "Test message" };

    var person = new Person
    {
      Id = new PersonId(Guid.NewGuid()),
      PlatformId = new PlatformId("testuser", "123", Platform.Discord),
      Username = new Username("testuser"),
      HasAccess = new HasAccess(true),
      NotificationChannels =
      [
        new(NotificationChannelType.Discord, "123456789", true)
      ],
      CreatedOn = new Domain.Common.ValueObjects.CreatedOn(DateTime.UtcNow),
      UpdatedOn = new Domain.Common.ValueObjects.UpdatedOn(DateTime.UtcNow)
    };

    // Act
    var result = await client.SendNotificationAsync(person, notification);

    // Assert
    _ = result.IsSuccess.Should().BeTrue();
    _ = await mockChannel.Received(1).SendAsync("123456789", notification, Arg.Any<CancellationToken>());
  }
}
