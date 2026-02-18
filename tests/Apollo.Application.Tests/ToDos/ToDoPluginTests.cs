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

namespace Apollo.Application.Tests.ToDos;

public sealed class ToDoPluginTests
{
  private readonly Mock<IMediator> _mediator = new();
  private readonly Mock<IPersonStore> _personStore = new();
  private readonly Mock<ITimeParsingService> _timeParsingService = new();
  private readonly PersonConfig _personConfig = new() { DefaultTimeZoneId = "America/Chicago" };
  private readonly PersonId _personId = new(Guid.NewGuid());

  private ToDoPlugin CreatePlugin() =>
    new(_mediator.Object, _personStore.Object, _timeParsingService.Object, _personConfig, _personId);

  [Fact]
  public async Task CreateToDoAsyncWithReminderDateCallsTimeParsingServiceAsync()
  {
    // Arrange
    var plugin = CreatePlugin();
    var reminderUtc = new DateTime(2026, 3, 1, 15, 0, 0, DateTimeKind.Utc);

    _ = _personStore.Setup(x => x.GetAsync(_personId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(CreatePerson()));

    _ = _timeParsingService.Setup(x => x.ParseTimeAsync("tomorrow at 3pm", "America/Chicago", It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(reminderUtc));

    _ = _mediator.Setup(m => m.Send(It.IsAny<CreateToDoCommand>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(CreateToDo("Buy milk")));

    // Act
    var result = await plugin.CreateToDoAsync("Buy milk", "tomorrow at 3pm");

    // Assert
    Assert.Contains("Successfully created todo", result);
    Assert.Contains("2026-03-01 15:00:00 UTC", result);
    _timeParsingService.Verify(x => x.ParseTimeAsync("tomorrow at 3pm", "America/Chicago", It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task CreateToDoAsyncWithoutReminderDoesNotCallTimeParsingServiceAsync()
  {
    // Arrange
    var plugin = CreatePlugin();

    _ = _mediator.Setup(m => m.Send(It.IsAny<CreateToDoCommand>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(CreateToDo("Buy milk")));

    // Act
    var result = await plugin.CreateToDoAsync("Buy milk");

    // Assert
    Assert.Contains("Successfully created todo", result);
    Assert.DoesNotContain("UTC", result);
    _timeParsingService.Verify(x => x.ParseTimeAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task CreateToDoAsyncWithFailedTimeParsingReturnsErrorAsync()
  {
    // Arrange
    var plugin = CreatePlugin();

    _ = _personStore.Setup(x => x.GetAsync(_personId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(CreatePerson()));

    _ = _timeParsingService.Setup(x => x.ParseTimeAsync("gibberish", "America/Chicago", It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Fail<DateTime>("Invalid time format."));

    // Act
    var result = await plugin.CreateToDoAsync("Buy milk", "gibberish");

    // Assert
    Assert.Contains("Failed to create todo", result);
    Assert.Contains("Invalid time format.", result);
    _mediator.Verify(m => m.Send(It.IsAny<CreateToDoCommand>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task CreateToDoAsyncUsesDefaultTimeZoneWhenPersonHasNoTimeZoneAsync()
  {
    // Arrange
    var plugin = CreatePlugin();
    var personWithoutTz = CreatePerson(timeZoneId: null);

    _ = _personStore.Setup(x => x.GetAsync(_personId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(personWithoutTz));

    _ = _timeParsingService.Setup(x => x.ParseTimeAsync("in 10 minutes", "America/Chicago", It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(DateTime.UtcNow.AddMinutes(10)));

    _ = _mediator.Setup(m => m.Send(It.IsAny<CreateToDoCommand>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(CreateToDo("Test")));

    // Act
    _ = await plugin.CreateToDoAsync("Test", "in 10 minutes");

    // Assert — should use default "America/Chicago" since person has no timezone
    _timeParsingService.Verify(x => x.ParseTimeAsync("in 10 minutes", "America/Chicago", It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task CreateToDoAsyncUsesPersonTimeZoneWhenAvailableAsync()
  {
    // Arrange
    var plugin = CreatePlugin();
    var personWithTz = CreatePerson(timeZoneId: "Europe/London");

    _ = _personStore.Setup(x => x.GetAsync(_personId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(personWithTz));

    _ = _timeParsingService.Setup(x => x.ParseTimeAsync("at 3pm", "Europe/London", It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(DateTime.UtcNow.AddHours(1)));

    _ = _mediator.Setup(m => m.Send(It.IsAny<CreateToDoCommand>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(CreateToDo("Test")));

    // Act
    _ = await plugin.CreateToDoAsync("Test", "at 3pm");

    // Assert — should use person's timezone "Europe/London"
    _timeParsingService.Verify(x => x.ParseTimeAsync("at 3pm", "Europe/London", It.IsAny<CancellationToken>()), Times.Once);
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

  private ToDo CreateToDo(string description)
  {
    return new ToDo
    {
      Id = new ToDoId(Guid.NewGuid()),
      PersonId = _personId,
      Description = new Description(description),
      Priority = new Priority(Level.Green),
      Energy = new Energy(Level.Green),
      Interest = new Interest(Level.Green),
      CreatedOn = new CreatedOn(DateTime.UtcNow),
      UpdatedOn = new UpdatedOn(DateTime.UtcNow)
    };
  }
}
