using Apollo.Application.Reminders;
using Apollo.Application.ToDos;
using Apollo.Core.People;
using Apollo.Core.ToDos;
using Apollo.Domain.Common.Enums;
using Apollo.Domain.Common.ValueObjects;
using Apollo.Domain.People.Models;
using Apollo.Domain.People.ValueObjects;
using Apollo.Domain.ToDos.Models;
using Apollo.Domain.ToDos.ValueObjects;

using FluentResults;

using MediatR;

using Moq;

namespace Apollo.Application.Tests.Reminders;

public sealed class RemindersPluginTests
{
  private readonly Mock<IMediator> _mediator = new();
  private readonly Mock<IPersonStore> _personStore = new();
  private readonly Mock<ITimeParsingService> _timeParsingService = new();
  private readonly PersonConfig _personConfig = new() { DefaultTimeZoneId = "America/Chicago" };
  private readonly PersonId _personId = new(Guid.NewGuid());

  private RemindersPlugin CreatePlugin() =>
    new(_mediator.Object, _personStore.Object, _timeParsingService.Object, _personConfig, _personId);

  [Fact]
  public async Task CreateReminderAsyncCallsTimeParsingServiceAsync()
  {
    // Arrange
    var plugin = CreatePlugin();
    var reminderUtc = new DateTime(2026, 3, 1, 15, 0, 0, DateTimeKind.Utc);

    _ = _personStore.Setup(x => x.GetAsync(_personId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(CreatePerson()));

    _ = _timeParsingService.Setup(x => x.ParseTimeAsync("in 10 minutes", "America/Chicago", It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(reminderUtc));

    _ = _mediator.Setup(m => m.Send(It.IsAny<CreateReminderCommand>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(CreateReminder("Take a break", reminderUtc)));

    // Act
    var result = await plugin.CreateReminderAsync("Take a break", "in 10 minutes");

    // Assert
    Assert.Contains("Successfully created reminder", result);
    Assert.Contains("2026-03-01 15:00:00 UTC", result);
    _timeParsingService.Verify(x => x.ParseTimeAsync("in 10 minutes", "America/Chicago", It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task CreateReminderAsyncWithEmptyTimeReturnsErrorAsync()
  {
    // Arrange
    var plugin = CreatePlugin();

    // Act
    var result = await plugin.CreateReminderAsync("Take a break", "");

    // Assert
    Assert.Contains("Failed to create reminder", result);
    Assert.Contains("Reminder time is required.", result);
    _timeParsingService.Verify(x => x.ParseTimeAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task CreateReminderAsyncWithFailedTimeParsingReturnsErrorAsync()
  {
    // Arrange
    var plugin = CreatePlugin();

    _ = _personStore.Setup(x => x.GetAsync(_personId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(CreatePerson()));

    _ = _timeParsingService.Setup(x => x.ParseTimeAsync("gibberish", "America/Chicago", It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Fail<DateTime>("Invalid time format."));

    // Act
    var result = await plugin.CreateReminderAsync("Take a break", "gibberish");

    // Assert
    Assert.Contains("Failed to create reminder", result);
    Assert.Contains("Invalid time format.", result);
    _mediator.Verify(m => m.Send(It.IsAny<CreateReminderCommand>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task CreateReminderAsyncUsesPersonTimeZoneWhenAvailableAsync()
  {
    // Arrange
    var plugin = CreatePlugin();
    var personWithTz = CreatePerson(timeZoneId: "Europe/London");
    var reminderUtc = DateTime.UtcNow.AddHours(1);

    _ = _personStore.Setup(x => x.GetAsync(_personId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(personWithTz));

    _ = _timeParsingService.Setup(x => x.ParseTimeAsync("at 3pm", "Europe/London", It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(reminderUtc));

    _ = _mediator.Setup(m => m.Send(It.IsAny<CreateReminderCommand>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(CreateReminder("Test", reminderUtc)));

    // Act
    _ = await plugin.CreateReminderAsync("Test", "at 3pm");

    // Assert — should use person's timezone "Europe/London"
    _timeParsingService.Verify(x => x.ParseTimeAsync("at 3pm", "Europe/London", It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task CreateReminderAsyncUsesDefaultTimeZoneWhenPersonHasNoTimeZoneAsync()
  {
    // Arrange
    var plugin = CreatePlugin();
    var personWithoutTz = CreatePerson(timeZoneId: null);
    var reminderUtc = DateTime.UtcNow.AddMinutes(30);

    _ = _personStore.Setup(x => x.GetAsync(_personId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(personWithoutTz));

    _ = _timeParsingService.Setup(x => x.ParseTimeAsync("in 30 minutes", "America/Chicago", It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(reminderUtc));

    _ = _mediator.Setup(m => m.Send(It.IsAny<CreateReminderCommand>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(CreateReminder("Test", reminderUtc)));

    // Act
    _ = await plugin.CreateReminderAsync("Test", "in 30 minutes");

    // Assert — should use default "America/Chicago"
    _timeParsingService.Verify(x => x.ParseTimeAsync("in 30 minutes", "America/Chicago", It.IsAny<CancellationToken>()), Times.Once);
  }

  private Person CreatePerson(string? timeZoneId = null)
  {
    PersonTimeZoneId? parsedTimeZoneId = null;
    if (timeZoneId is not null && PersonTimeZoneId.TryParse(timeZoneId, out var tzId, out _))
    {
      parsedTimeZoneId = tzId;
    }

    return new Person
    {
      Id = _personId,
      PlatformId = new PlatformId("testuser", "123", Platform.Discord),
      Username = new Username("testuser"),
      HasAccess = new HasAccess(true),
      TimeZoneId = parsedTimeZoneId,
      CreatedOn = new CreatedOn(DateTime.UtcNow),
      UpdatedOn = new UpdatedOn(DateTime.UtcNow)
    };
  }

  private Reminder CreateReminder(string details, DateTime reminderTime)
  {
    return new Reminder
    {
      Id = new ReminderId(Guid.NewGuid()),
      PersonId = _personId,
      Details = new Details(details),
      ReminderTime = new ReminderTime(reminderTime),
      CreatedOn = new CreatedOn(DateTime.UtcNow),
      UpdatedOn = new UpdatedOn(DateTime.UtcNow)
    };
  }
}
