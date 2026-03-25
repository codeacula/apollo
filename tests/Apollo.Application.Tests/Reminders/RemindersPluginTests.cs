using Apollo.Application.Reminders;
using Apollo.Application.Tests.TestSupport;
using Apollo.Application.ToDos;
using Apollo.Core.People;
using Apollo.Core.ToDos;
using Apollo.Domain.People.ValueObjects;

using FluentResults;

using MediatR;

using Moq;

namespace Apollo.Application.Tests.Reminders;

public sealed class RemindersPluginTests
{
  public static TheoryData<string?, string, string> TimezoneCases => new()
  {
    { "Europe/London", "at 3pm", "Europe/London" },
    { null, "in 30 minutes", "America/Chicago" }
  };

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
      .ReturnsAsync(Result.Ok(ApplicationTestData.CreatePerson(_personId)));

    _ = _timeParsingService.Setup(x => x.ParseTimeAsync("in 10 minutes", "America/Chicago", It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(reminderUtc));

    _ = _mediator.Setup(m => m.Send(It.IsAny<CreateReminderCommand>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(ApplicationTestData.CreateReminder(_personId, "Take a break", reminderTime: reminderUtc)));

    // Act
    var result = await plugin.CreateReminderAsync("Take a break", "in 10 minutes");

    // Assert
    Assert.Contains("Successfully created reminder", result);
    Assert.Contains("2026-03-01 15:00:00 UTC", result);
    _timeParsingService.Verify(x => x.ParseTimeAsync("in 10 minutes", "America/Chicago", It.IsAny<CancellationToken>()), Times.Once);
  }

  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  public async Task CreateReminderAsyncWithMissingTimeReturnsErrorAsync(string reminderTime)
  {
    // Arrange
    var plugin = CreatePlugin();

    // Act
    var result = await plugin.CreateReminderAsync("Take a break", reminderTime);

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
      .ReturnsAsync(Result.Ok(ApplicationTestData.CreatePerson(_personId)));

    _ = _timeParsingService.Setup(x => x.ParseTimeAsync("gibberish", "America/Chicago", It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Fail<DateTime>("Invalid time format."));

    // Act
    var result = await plugin.CreateReminderAsync("Take a break", "gibberish");

    // Assert
    Assert.Contains("Failed to create reminder", result);
    Assert.Contains("Invalid time format.", result);
    _mediator.Verify(m => m.Send(It.IsAny<CreateReminderCommand>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Theory]
  [MemberData(nameof(TimezoneCases))]
  public async Task CreateReminderAsyncUsesExpectedTimezoneAsync(string? personTimeZoneId, string reminderTime, string expectedTimezone)
  {
    // Arrange
    var plugin = CreatePlugin();
    var reminderUtc = DateTime.UtcNow.AddHours(1);
    var person = ApplicationTestData.CreatePerson(_personId, timeZoneId: personTimeZoneId);

    _ = _personStore.Setup(x => x.GetAsync(_personId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(person));

    _ = _timeParsingService.Setup(x => x.ParseTimeAsync(reminderTime, expectedTimezone, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(reminderUtc));

    _ = _mediator.Setup(m => m.Send(It.IsAny<CreateReminderCommand>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(ApplicationTestData.CreateReminder(_personId, "Test", reminderTime: reminderUtc)));

    // Act
    _ = await plugin.CreateReminderAsync("Test", reminderTime);

    _timeParsingService.Verify(x => x.ParseTimeAsync(reminderTime, expectedTimezone, It.IsAny<CancellationToken>()), Times.Once);
  }
}
