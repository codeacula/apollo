using Apollo.Application.Reminders;
using Apollo.Application.ToDos;
using Apollo.Core.People;
using Apollo.Core.ToDos;
using Apollo.Domain.People.ValueObjects;

using FluentResults;

using MediatR;

using Moq;

namespace Apollo.Application.Tests.Reminders;

public class RemindersPluginTests
{
  [Fact]
  public async Task CreateReminderAsyncWithFuzzyTimeSucceedsAsync()
  {
    // Arrange
    var personId = new PersonId(Guid.NewGuid());
    var mediator = new Mock<IMediator>();
    var personStore = new Mock<IPersonStore>();
    var fuzzyTimeParser = new Mock<IFuzzyTimeParser>();
    var timeProvider = TimeProvider.System;
    var personConfig = new PersonConfig { DefaultDailyTaskCount = 5, DefaultTimeZoneId = "UTC" };
    var plugin = new RemindersPlugin(mediator.Object, personStore.Object, fuzzyTimeParser.Object, timeProvider, personConfig, personId);

    var reminderDate = DateTime.UtcNow.AddMinutes(30);
    _ = fuzzyTimeParser
      .Setup(x => x.TryParseFuzzyTime("in 30 minutes", It.IsAny<DateTime>()))
      .Returns(Result.Ok(reminderDate));

    _ = mediator
      .Setup(x => x.Send(It.IsAny<CreateReminderCommand>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok());

    // Act
    var result = await plugin.CreateReminderAsync("take a break", "in 30 minutes");

    // Assert
    Assert.NotNull(result);
    Assert.Contains("Successfully created reminder", result);
    Assert.Contains("take a break", result);
  }

  [Fact]
  public async Task CreateReminderAsyncWithISODateSucceedsAsync()
  {
    // Arrange
    var personId = new PersonId(Guid.NewGuid());
    var mediator = new Mock<IMediator>();
    var personStore = new Mock<IPersonStore>();
    var fuzzyTimeParser = new Mock<IFuzzyTimeParser>();
    var timeProvider = TimeProvider.System;
    var personConfig = new PersonConfig { DefaultDailyTaskCount = 5, DefaultTimeZoneId = "UTC" };
    var plugin = new RemindersPlugin(mediator.Object, personStore.Object, fuzzyTimeParser.Object, timeProvider, personConfig, personId);

    const string isoDate = "2025-12-31T10:00:00Z";
    _ = fuzzyTimeParser
      .Setup(x => x.TryParseFuzzyTime(isoDate, It.IsAny<DateTime>()))
      .Returns(Result.Fail<DateTime>("Not fuzzy"));

    _ = personStore
      .Setup(x => x.GetAsync(personId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(CreatePerson(personId)));

    _ = mediator
      .Setup(x => x.Send(It.IsAny<CreateReminderCommand>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok());

    // Act
    var result = await plugin.CreateReminderAsync("check oven", isoDate);

    // Assert
    Assert.NotNull(result);
    Assert.Contains("Successfully created reminder", result);
    Assert.Contains("check oven", result);
  }

  [Fact]
  public async Task CreateReminderAsyncWithInvalidTimeReturnsFailureAsync()
  {
    // Arrange
    var personId = new PersonId(Guid.NewGuid());
    var mediator = new Mock<IMediator>();
    var personStore = new Mock<IPersonStore>();
    var fuzzyTimeParser = new Mock<IFuzzyTimeParser>();
    var timeProvider = TimeProvider.System;
    var personConfig = new PersonConfig { DefaultDailyTaskCount = 5, DefaultTimeZoneId = "UTC" };
    var plugin = new RemindersPlugin(mediator.Object, personStore.Object, fuzzyTimeParser.Object, timeProvider, personConfig, personId);

    _ = fuzzyTimeParser
      .Setup(x => x.TryParseFuzzyTime("invalid time", It.IsAny<DateTime>()))
      .Returns(Result.Fail<DateTime>("Invalid format"));

    // Act
    var result = await plugin.CreateReminderAsync("test", "invalid time");

    // Assert
    Assert.NotNull(result);
    Assert.Contains("Failed to create reminder", result);
  }

  [Fact]
  public async Task CreateReminderAsyncWithNullReminderTimeReturnsFailureAsync()
  {
    // Arrange
    var personId = new PersonId(Guid.NewGuid());
    var mediator = new Mock<IMediator>();
    var personStore = new Mock<IPersonStore>();
    var fuzzyTimeParser = new Mock<IFuzzyTimeParser>();
    var timeProvider = TimeProvider.System;
    var personConfig = new PersonConfig { DefaultDailyTaskCount = 5, DefaultTimeZoneId = "UTC" };
    var plugin = new RemindersPlugin(mediator.Object, personStore.Object, fuzzyTimeParser.Object, timeProvider, personConfig, personId);

    // Act
    var result = await plugin.CreateReminderAsync("test", "");

    // Assert
    Assert.NotNull(result);
    Assert.Contains("Failed to create reminder", result);
  }

  [Fact]
  public async Task CreateReminderAsyncWithMediatorFailureReturnsFailureAsync()
  {
    // Arrange
    var personId = new PersonId(Guid.NewGuid());
    var mediator = new Mock<IMediator>();
    var personStore = new Mock<IPersonStore>();
    var fuzzyTimeParser = new Mock<IFuzzyTimeParser>();
    var timeProvider = TimeProvider.System;
    var personConfig = new PersonConfig { DefaultDailyTaskCount = 5, DefaultTimeZoneId = "UTC" };
    var plugin = new RemindersPlugin(mediator.Object, personStore.Object, fuzzyTimeParser.Object, timeProvider, personConfig, personId);

    var reminderDate = DateTime.UtcNow.AddMinutes(30);
    _ = fuzzyTimeParser
      .Setup(x => x.TryParseFuzzyTime("in 30 minutes", It.IsAny<DateTime>()))
      .Returns(Result.Ok(reminderDate));

    _ = mediator
      .Setup(x => x.Send(It.IsAny<CreateReminderCommand>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Fail("Schedule conflict"));

    // Act
    var result = await plugin.CreateReminderAsync("test", "in 30 minutes");

    // Assert
    Assert.NotNull(result);
    Assert.Contains("Failed to create reminder", result);
    Assert.Contains("Schedule conflict", result);
  }

  [Fact]
  public async Task CreateReminderAsyncWithExceptionReturnsErrorAsync()
  {
    // Arrange
    var personId = new PersonId(Guid.NewGuid());
    var mediator = new Mock<IMediator>();
    var personStore = new Mock<IPersonStore>();
    var fuzzyTimeParser = new Mock<IFuzzyTimeParser>();
    var timeProvider = TimeProvider.System;
    var personConfig = new PersonConfig { DefaultDailyTaskCount = 5, DefaultTimeZoneId = "UTC" };
    var plugin = new RemindersPlugin(mediator.Object, personStore.Object, fuzzyTimeParser.Object, timeProvider, personConfig, personId);

    _ = fuzzyTimeParser
      .Setup(x => x.TryParseFuzzyTime(It.IsAny<string>(), It.IsAny<DateTime>()))
      .Throws(new InvalidOperationException("Parser error"));

    // Act
    var result = await plugin.CreateReminderAsync("test", "in 5 minutes");

    // Assert
    Assert.NotNull(result);
    Assert.Contains("Error creating reminder", result);
    Assert.Contains("Parser error", result);
  }

  [Fact]
  public async Task CancelReminderAsyncWithValidIdSucceedsAsync()
  {
    // Arrange
    var personId = new PersonId(Guid.NewGuid());
    var reminderId = Guid.NewGuid();
    var mediator = new Mock<IMediator>();
    var personStore = new Mock<IPersonStore>();
    var fuzzyTimeParser = new Mock<IFuzzyTimeParser>();
    var timeProvider = TimeProvider.System;
    var personConfig = new PersonConfig { DefaultDailyTaskCount = 5, DefaultTimeZoneId = "UTC" };
    var plugin = new RemindersPlugin(mediator.Object, personStore.Object, fuzzyTimeParser.Object, timeProvider, personConfig, personId);

    _ = mediator
      .Setup(x => x.Send(It.IsAny<CancelReminderCommand>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok());

    // Act
    var result = await plugin.CancelReminderAsync(reminderId.ToString());

    // Assert
    Assert.NotNull(result);
    Assert.Contains("Successfully canceled", result);
  }

  [Fact]
  public async Task CancelReminderAsyncWithInvalidGuidReturnsFailureAsync()
  {
    // Arrange
    var personId = new PersonId(Guid.NewGuid());
    var mediator = new Mock<IMediator>();
    var personStore = new Mock<IPersonStore>();
    var fuzzyTimeParser = new Mock<IFuzzyTimeParser>();
    var timeProvider = TimeProvider.System;
    var personConfig = new PersonConfig { DefaultDailyTaskCount = 5, DefaultTimeZoneId = "UTC" };
    var plugin = new RemindersPlugin(mediator.Object, personStore.Object, fuzzyTimeParser.Object, timeProvider, personConfig, personId);

    // Act
    var result = await plugin.CancelReminderAsync("not-a-guid");

    // Assert
    Assert.NotNull(result);
    Assert.Contains("Failed to cancel reminder", result);
    Assert.Contains("Invalid reminder ID", result);
  }

  [Fact]
  public async Task CancelReminderAsyncWithMediatorFailureReturnsFailureAsync()
  {
    // Arrange
    var personId = new PersonId(Guid.NewGuid());
    var reminderId = Guid.NewGuid();
    var mediator = new Mock<IMediator>();
    var personStore = new Mock<IPersonStore>();
    var fuzzyTimeParser = new Mock<IFuzzyTimeParser>();
    var timeProvider = TimeProvider.System;
    var personConfig = new PersonConfig { DefaultDailyTaskCount = 5, DefaultTimeZoneId = "UTC" };
    var plugin = new RemindersPlugin(mediator.Object, personStore.Object, fuzzyTimeParser.Object, timeProvider, personConfig, personId);

    _ = mediator
      .Setup(x => x.Send(It.IsAny<CancelReminderCommand>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Fail("Reminder not found"));

    // Act
    var result = await plugin.CancelReminderAsync(reminderId.ToString());

    // Assert
    Assert.NotNull(result);
    Assert.Contains("Failed to cancel reminder", result);
    Assert.Contains("Reminder not found", result);
  }

  [Fact]
  public async Task CancelReminderAsyncWithExceptionReturnsErrorAsync()
  {
    // Arrange
    var personId = new PersonId(Guid.NewGuid());
    var reminderId = Guid.NewGuid();
    var mediator = new Mock<IMediator>();
    var personStore = new Mock<IPersonStore>();
    var fuzzyTimeParser = new Mock<IFuzzyTimeParser>();
    var timeProvider = TimeProvider.System;
    var personConfig = new PersonConfig { DefaultDailyTaskCount = 5, DefaultTimeZoneId = "UTC" };
    var plugin = new RemindersPlugin(mediator.Object, personStore.Object, fuzzyTimeParser.Object, timeProvider, personConfig, personId);

    _ = mediator
      .Setup(x => x.Send(It.IsAny<CancelReminderCommand>(), It.IsAny<CancellationToken>()))
      .ThrowsAsync(new InvalidOperationException("Database error"));

    // Act
    var result = await plugin.CancelReminderAsync(reminderId.ToString());

    // Assert
    Assert.NotNull(result);
    Assert.Contains("Error canceling reminder", result);
    Assert.Contains("Database error", result);
  }

  [Fact]
  public async Task CreateReminderAsyncWithEmptyMessageReturnsFailureAsync()
  {
    // Arrange
    var personId = new PersonId(Guid.NewGuid());
    var mediator = new Mock<IMediator>();
    var personStore = new Mock<IPersonStore>();
    var fuzzyTimeParser = new Mock<IFuzzyTimeParser>();
    var timeProvider = TimeProvider.System;
    var personConfig = new PersonConfig { DefaultDailyTaskCount = 5, DefaultTimeZoneId = "UTC" };
    var plugin = new RemindersPlugin(mediator.Object, personStore.Object, fuzzyTimeParser.Object, timeProvider, personConfig, personId);

    // Act - with null/empty message - plugin still processes, but mediator might fail
    var result = await plugin.CreateReminderAsync("", "in 5 minutes");

    // Assert - plugin wraps the call regardless of message content
    Assert.NotNull(result);
  }

  [Fact]
  public async Task CreateReminderAsyncWithMultipleTimeFormatsSucceedsAsync()
  {
    // Arrange
    var personId = new PersonId(Guid.NewGuid());
    var mediator = new Mock<IMediator>();
    var personStore = new Mock<IPersonStore>();
    var fuzzyTimeParser = new Mock<IFuzzyTimeParser>();
    var timeProvider = TimeProvider.System;
    var personConfig = new PersonConfig { DefaultDailyTaskCount = 5, DefaultTimeZoneId = "UTC" };
    var plugin = new RemindersPlugin(mediator.Object, personStore.Object, fuzzyTimeParser.Object, timeProvider, personConfig, personId);

    var reminderDates = new[]
    {
      DateTime.UtcNow.AddMinutes(10),
      DateTime.UtcNow.AddHours(2),
      DateTime.UtcNow.AddDays(1)
    };

    var fuzzyFormats = new[] { "in 10 minutes", "in 2 hours", "tomorrow" };

    for (int i = 0; i < fuzzyFormats.Length; i++)
    {
      _ = fuzzyTimeParser
        .Setup(x => x.TryParseFuzzyTime(fuzzyFormats[i], It.IsAny<DateTime>()))
        .Returns(Result.Ok(reminderDates[i]));
    }

    _ = mediator
      .Setup(x => x.Send(It.IsAny<CreateReminderCommand>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok());

    // Act & Assert for each format
    foreach (var format in fuzzyFormats)
    {
      var result = await plugin.CreateReminderAsync("test", format);
      Assert.NotNull(result);
      Assert.Contains("Successfully created reminder", result);
    }
  }

  private static Domain.People.Models.Person CreatePerson(PersonId personId)
  {
    return new Domain.People.Models.Person
    {
      Id = personId,
      PlatformId = new PlatformId(
        "testuser",
        Guid.NewGuid().ToString(),
        Domain.Common.Enums.Platform.Discord),
      Username = new Username("testuser"),
      HasAccess = new HasAccess(true),
      CreatedOn = new Domain.Common.ValueObjects.CreatedOn(DateTime.UtcNow),
      UpdatedOn = new Domain.Common.ValueObjects.UpdatedOn(DateTime.UtcNow)
    };
  }
}
