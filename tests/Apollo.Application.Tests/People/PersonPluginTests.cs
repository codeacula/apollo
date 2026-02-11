using Apollo.Application.People;
using Apollo.Core.People;
using Apollo.Domain.Common.ValueObjects;
using Apollo.Domain.People.Models;
using Apollo.Domain.People.ValueObjects;

using FluentResults;

using Moq;

namespace Apollo.Application.Tests.People;

public class PersonPluginTests
{
  [Fact]
  public async Task SetTimeZoneAsyncWithValidTimeZoneSucceedsAsync()
  {
    // Arrange
    var personId = new PersonId(Guid.NewGuid());
    var personStore = new Mock<IPersonStore>();
    var personConfig = new PersonConfig { DefaultDailyTaskCount = 5, DefaultTimeZoneId = "UTC" };
    var plugin = new PersonPlugin(personStore.Object, personConfig, personId);

    _ = personStore
      .Setup(x => x.SetTimeZoneAsync(personId, It.IsAny<PersonTimeZoneId>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok());

    // Act
    var result = await plugin.SetTimeZoneAsync("America/New_York");

    // Assert
    Assert.NotNull(result);
    Assert.Contains("Successfully set your timezone", result);
    Assert.Contains("America/New_York", result);
  }

  [Fact]
  public async Task SetTimeZoneAsyncWithCommonAbbreviationSucceedsAsync()
  {
    // Arrange
    var personId = new PersonId(Guid.NewGuid());
    var personStore = new Mock<IPersonStore>();
    var personConfig = new PersonConfig { DefaultDailyTaskCount = 5, DefaultTimeZoneId = "UTC" };
    var plugin = new PersonPlugin(personStore.Object, personConfig, personId);

    _ = personStore
      .Setup(x => x.SetTimeZoneAsync(personId, It.IsAny<PersonTimeZoneId>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok());

    // Act
    var result = await plugin.SetTimeZoneAsync("EST");

    // Assert
    Assert.NotNull(result);
    Assert.Contains("Successfully set your timezone", result);
  }

  [Fact]
  public async Task SetTimeZoneAsyncWithInvalidTimeZoneFailsAsync()
  {
    // Arrange
    var personId = new PersonId(Guid.NewGuid());
    var personStore = new Mock<IPersonStore>();
    var personConfig = new PersonConfig { DefaultDailyTaskCount = 5, DefaultTimeZoneId = "UTC" };
    var plugin = new PersonPlugin(personStore.Object, personConfig, personId);

    // Act
    var result = await plugin.SetTimeZoneAsync("Invalid/Timezone");

    // Assert
    Assert.NotNull(result);
    Assert.Contains("Failed to set timezone", result);
  }

  [Fact]
  public async Task SetTimeZoneAsyncWithStoreFailureReturnsFailureMessageAsync()
  {
    // Arrange
    var personId = new PersonId(Guid.NewGuid());
    var personStore = new Mock<IPersonStore>();
    var personConfig = new PersonConfig { DefaultDailyTaskCount = 5, DefaultTimeZoneId = "UTC" };
    var plugin = new PersonPlugin(personStore.Object, personConfig, personId);

    _ = personStore
      .Setup(x => x.SetTimeZoneAsync(personId, It.IsAny<PersonTimeZoneId>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Fail("Database error"));

    // Act
    var result = await plugin.SetTimeZoneAsync("America/Chicago");

    // Assert
    Assert.NotNull(result);
    Assert.Contains("Failed to set timezone", result);
    Assert.Contains("Database error", result);
  }

  [Fact]
  public async Task SetTimeZoneAsyncWithExceptionReturnsErrorMessageAsync()
  {
    // Arrange
    var personId = new PersonId(Guid.NewGuid());
    var personStore = new Mock<IPersonStore>();
    var personConfig = new PersonConfig { DefaultDailyTaskCount = 5, DefaultTimeZoneId = "UTC" };
    var plugin = new PersonPlugin(personStore.Object, personConfig, personId);

    _ = personStore
      .Setup(x => x.SetTimeZoneAsync(personId, It.IsAny<PersonTimeZoneId>(), It.IsAny<CancellationToken>()))
      .ThrowsAsync(new InvalidOperationException("Connection failed"));

    // Act
    var result = await plugin.SetTimeZoneAsync("America/Los_Angeles");

    // Assert
    Assert.NotNull(result);
    Assert.Contains("Error setting timezone", result);
    Assert.Contains("Connection failed", result);
  }

  [Fact]
  public async Task GetTimeZoneAsyncWithUserTimeZoneReturnsSettingAsync()
  {
    // Arrange
    var personId = new PersonId(Guid.NewGuid());
    var personStore = new Mock<IPersonStore>();
    var personConfig = new PersonConfig { DefaultDailyTaskCount = 5, DefaultTimeZoneId = "UTC" };
    var plugin = new PersonPlugin(personStore.Object, personConfig, personId);

    var person = CreatePersonWithTimeZone(personId, "America/New_York");
    _ = personStore
      .Setup(x => x.GetAsync(personId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(person));

    // Act
    var result = await plugin.GetTimeZoneAsync();

    // Assert
    Assert.NotNull(result);
    Assert.Contains("Your timezone is set to", result);
    Assert.Contains("America/New_York", result);
  }

  [Fact]
  public async Task GetTimeZoneAsyncWithDefaultTimeZoneReturnsDefaultAsync()
  {
    // Arrange
    var personId = new PersonId(Guid.NewGuid());
    var personStore = new Mock<IPersonStore>();
    var personConfig = new PersonConfig { DefaultDailyTaskCount = 5, DefaultTimeZoneId = "Europe/London" };
    var plugin = new PersonPlugin(personStore.Object, personConfig, personId);

    var person = CreatePersonWithoutTimeZone(personId);
    _ = personStore
      .Setup(x => x.GetAsync(personId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(person));

    // Act
    var result = await plugin.GetTimeZoneAsync();

    // Assert
    Assert.NotNull(result);
    Assert.Contains("default timezone", result);
    Assert.Contains("Europe/London", result);
  }

  [Fact]
  public async Task GetTimeZoneAsyncWithStoreFailureReturnsFailureMessageAsync()
  {
    // Arrange
    var personId = new PersonId(Guid.NewGuid());
    var personStore = new Mock<IPersonStore>();
    var personConfig = new PersonConfig { DefaultDailyTaskCount = 5, DefaultTimeZoneId = "UTC" };
    var plugin = new PersonPlugin(personStore.Object, personConfig, personId);

    _ = personStore
      .Setup(x => x.GetAsync(personId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Fail<Person>("Person not found"));

    // Act
    var result = await plugin.GetTimeZoneAsync();

    // Assert
    Assert.NotNull(result);
    Assert.Contains("Failed to retrieve timezone", result);
    Assert.Contains("Person not found", result);
  }

  [Fact]
  public async Task GetTimeZoneAsyncWithExceptionReturnsErrorMessageAsync()
  {
    // Arrange
    var personId = new PersonId(Guid.NewGuid());
    var personStore = new Mock<IPersonStore>();
    var personConfig = new PersonConfig { DefaultDailyTaskCount = 5, DefaultTimeZoneId = "UTC" };
    var plugin = new PersonPlugin(personStore.Object, personConfig, personId);

    _ = personStore
      .Setup(x => x.GetAsync(personId, It.IsAny<CancellationToken>()))
      .ThrowsAsync(new InvalidOperationException("Database connection error"));

    // Act
    var result = await plugin.GetTimeZoneAsync();

    // Assert
    Assert.NotNull(result);
    Assert.Contains("Error retrieving timezone", result);
    Assert.Contains("Database connection error", result);
  }

  [Fact]
  public async Task SetDailyTaskCountAsyncWithValidCountSucceedsAsync()
  {
    // Arrange
    var personId = new PersonId(Guid.NewGuid());
    var personStore = new Mock<IPersonStore>();
    var personConfig = new PersonConfig { DefaultDailyTaskCount = 5, DefaultTimeZoneId = "UTC" };
    var plugin = new PersonPlugin(personStore.Object, personConfig, personId);

    _ = personStore
      .Setup(x => x.SetDailyTaskCountAsync(personId, It.IsAny<DailyTaskCount>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok());

    // Act
    var result = await plugin.SetDailyTaskCountAsync(7);

    // Assert
    Assert.NotNull(result);
    Assert.Contains("Successfully set your daily task count", result);
    Assert.Contains("7", result);
  }

  [Fact]
  public async Task SetDailyTaskCountAsyncWithInvalidCountFailsAsync()
  {
    // Arrange
    var personId = new PersonId(Guid.NewGuid());
    var personStore = new Mock<IPersonStore>();
    var personConfig = new PersonConfig { DefaultDailyTaskCount = 5, DefaultTimeZoneId = "UTC" };
    var plugin = new PersonPlugin(personStore.Object, personConfig, personId);

    // Act - count outside valid range (1-20)
    var result = await plugin.SetDailyTaskCountAsync(25);

    // Assert
    Assert.NotNull(result);
    Assert.Contains("Failed to set daily task count", result);
  }

  [Fact]
  public async Task GetDailyTaskCountAsyncWithUserCountReturnsSettingAsync()
  {
    // Arrange
    var personId = new PersonId(Guid.NewGuid());
    var personStore = new Mock<IPersonStore>();
    var personConfig = new PersonConfig { DefaultDailyTaskCount = 5, DefaultTimeZoneId = "UTC" };
    var plugin = new PersonPlugin(personStore.Object, personConfig, personId);

    var person = CreatePersonWithDailyTaskCount(personId, 8);
    _ = personStore
      .Setup(x => x.GetAsync(personId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(person));

    // Act
    var result = await plugin.GetDailyTaskCountAsync();

    // Assert
    Assert.NotNull(result);
    Assert.Contains("Your daily task count is set to", result);
    Assert.Contains("8", result);
  }

  [Fact]
  public async Task GetDailyTaskCountAsyncWithDefaultCountReturnsDefaultAsync()
  {
    // Arrange
    var personId = new PersonId(Guid.NewGuid());
    var personStore = new Mock<IPersonStore>();
    var personConfig = new PersonConfig { DefaultDailyTaskCount = 5, DefaultTimeZoneId = "UTC" };
    var plugin = new PersonPlugin(personStore.Object, personConfig, personId);

    var person = CreatePersonWithoutDailyTaskCount(personId);
    _ = personStore
      .Setup(x => x.GetAsync(personId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(person));

    // Act
    var result = await plugin.GetDailyTaskCountAsync();

    // Assert
    Assert.NotNull(result);
    Assert.Contains("default daily task count", result);
    Assert.Contains("5", result);
  }

  private static Person CreatePersonWithTimeZone(PersonId personId, string timeZoneId)
  {
    _ = PersonTimeZoneId.TryParse(timeZoneId, out var tz, out _);
    return new Person
    {
      Id = personId,
      PlatformId = new PlatformId("testuser", Guid.NewGuid().ToString(), Domain.Common.Enums.Platform.Discord),
      Username = new Username("testuser"),
      HasAccess = new HasAccess(true),
      TimeZoneId = tz,
      CreatedOn = new CreatedOn(DateTime.UtcNow),
      UpdatedOn = new UpdatedOn(DateTime.UtcNow)
    };
  }

  private static Person CreatePersonWithoutTimeZone(PersonId personId)
  {
    return new Person
    {
      Id = personId,
      PlatformId = new PlatformId("testuser", Guid.NewGuid().ToString(), Domain.Common.Enums.Platform.Discord),
      Username = new Username("testuser"),
      HasAccess = new HasAccess(true),
      TimeZoneId = null,
      CreatedOn = new CreatedOn(DateTime.UtcNow),
      UpdatedOn = new UpdatedOn(DateTime.UtcNow)
    };
  }

  private static Person CreatePersonWithDailyTaskCount(PersonId personId, int count)
  {
    return new Person
    {
      Id = personId,
      PlatformId = new PlatformId("testuser", Guid.NewGuid().ToString(), Domain.Common.Enums.Platform.Discord),
      Username = new Username("testuser"),
      HasAccess = new HasAccess(true),
      DailyTaskCount = new DailyTaskCount(count),
      CreatedOn = new CreatedOn(DateTime.UtcNow),
      UpdatedOn = new UpdatedOn(DateTime.UtcNow)
    };
  }

  private static Person CreatePersonWithoutDailyTaskCount(PersonId personId)
  {
    return new Person
    {
      Id = personId,
      PlatformId = new PlatformId("testuser", Guid.NewGuid().ToString(), Domain.Common.Enums.Platform.Discord),
      Username = new Username("testuser"),
      HasAccess = new HasAccess(true),
      DailyTaskCount = null,
      CreatedOn = new CreatedOn(DateTime.UtcNow),
      UpdatedOn = new UpdatedOn(DateTime.UtcNow)
    };
  }
}
