using Apollo.Domain.Common.Enums;
using Apollo.GRPC.Contracts;

namespace Apollo.GRPC.Tests.Contracts;

public class SetToDoEnergyRequestTests
{
  [Fact]
  public void RequiredPropertiesAreSetCorrectly()
  {
    // Arrange
    const string username = "testuser";
    const string platformUserId = "12345";
    const Platform platform = Platform.Discord;
    var toDoId = Guid.NewGuid();
    const Level energy = Level.Yellow;

    // Act
    var request = new SetToDoEnergyRequest
    {
      Username = username,
      PlatformUserId = platformUserId,
      Platform = platform,
      ToDoId = toDoId,
      Energy = energy
    };

    // Assert
    Assert.Equal(username, request.Username);
    Assert.Equal(platformUserId, request.PlatformUserId);
    Assert.Equal(platform, request.Platform);
    Assert.Equal(toDoId, request.ToDoId);
    Assert.Equal(energy, request.Energy);
  }

  [Fact]
  public void AllPropertiesCanBeSetViaInitializer()
  {
    // Arrange
    var expectedId = Guid.NewGuid();

    // Act
    var request = new SetToDoEnergyRequest
    {
      Username = "john",
      PlatformUserId = "999",
      Platform = Platform.Discord,
      ToDoId = expectedId,
      Energy = Level.Red
    };

    // Assert
    Assert.NotNull(request);
    Assert.Equal("john", request.Username);
    Assert.Equal("999", request.PlatformUserId);
    Assert.Equal(Platform.Discord, request.Platform);
    Assert.Equal(expectedId, request.ToDoId);
    Assert.Equal(Level.Red, request.Energy);
  }

  [Theory]
  [InlineData(Level.Blue)]
  [InlineData(Level.Green)]
  [InlineData(Level.Yellow)]
  [InlineData(Level.Red)]
  public void EnergyPropertyAcceptsAllLevelValues(Level level)
  {
    // Arrange & Act
    var request = new SetToDoEnergyRequest
    {
      Username = "user",
      PlatformUserId = "123",
      Platform = Platform.Discord,
      ToDoId = Guid.NewGuid(),
      Energy = level
    };

    // Assert
    Assert.Equal(level, request.Energy);
  }
}
