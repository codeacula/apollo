using Apollo.Application.Tests.TestSupport;
using Apollo.Application.ToDos;
using Apollo.Core.People;
using Apollo.Core.ToDos;
using Apollo.Domain.People.ValueObjects;

using FluentResults;

using MediatR;

using Moq;

namespace Apollo.Application.Tests.ToDos;

public sealed class ToDoPluginTests
{
  public static TheoryData<string?, string, string> ReminderTimezoneCases => new()
  {
    { null, "in 10 minutes", "America/Chicago" },
    { "Europe/London", "at 3pm", "Europe/London" }
  };

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
      .ReturnsAsync(Result.Ok(ApplicationTestData.CreatePerson(_personId)));

    _ = _timeParsingService.Setup(x => x.ParseTimeAsync("tomorrow at 3pm", "America/Chicago", It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(reminderUtc));

    _ = _mediator.Setup(m => m.Send(It.IsAny<CreateToDoCommand>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(ApplicationTestData.CreateToDo(_personId, "Buy milk")));

    // Act
    var result = await plugin.CreateToDoAsync("Buy milk", "tomorrow at 3pm");

    // Assert
    Assert.Contains("Successfully created todo", result);
    Assert.Contains("2026-03-01 15:00:00 UTC", result);
    _timeParsingService.Verify(x => x.ParseTimeAsync("tomorrow at 3pm", "America/Chicago", It.IsAny<CancellationToken>()), Times.Once);
  }

  [Theory]
  [InlineData(null)]
  [InlineData("   ")]
  public async Task CreateToDoAsyncWithoutParsableReminderDoesNotCallTimeParsingServiceAsync(string? reminderDate)
  {
    // Arrange
    var plugin = CreatePlugin();

    _ = _mediator.Setup(m => m.Send(It.IsAny<CreateToDoCommand>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(ApplicationTestData.CreateToDo(_personId, "Buy milk")));

    // Act
    var result = reminderDate is null
      ? await plugin.CreateToDoAsync("Buy milk")
      : await plugin.CreateToDoAsync("Buy milk", reminderDate);

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
      .ReturnsAsync(Result.Ok(ApplicationTestData.CreatePerson(_personId)));

    _ = _timeParsingService.Setup(x => x.ParseTimeAsync("gibberish", "America/Chicago", It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Fail<DateTime>("Invalid time format."));

    // Act
    var result = await plugin.CreateToDoAsync("Buy milk", "gibberish");

    // Assert
    Assert.Contains("Failed to create todo", result);
    Assert.Contains("Invalid time format.", result);
    _mediator.Verify(m => m.Send(It.IsAny<CreateToDoCommand>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Theory]
  [MemberData(nameof(ReminderTimezoneCases))]
  public async Task CreateToDoAsyncUsesExpectedTimezoneAsync(string? personTimeZoneId, string reminderDate, string expectedTimezone)
  {
    // Arrange
    var plugin = CreatePlugin();
    var person = ApplicationTestData.CreatePerson(_personId, timeZoneId: personTimeZoneId);

    _ = _personStore.Setup(x => x.GetAsync(_personId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(person));

    _ = _timeParsingService.Setup(x => x.ParseTimeAsync(reminderDate, expectedTimezone, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(DateTime.UtcNow.AddMinutes(10)));

    _ = _mediator.Setup(m => m.Send(It.IsAny<CreateToDoCommand>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(ApplicationTestData.CreateToDo(_personId, "Test")));

    // Act
    _ = await plugin.CreateToDoAsync("Test", reminderDate);

    _timeParsingService.Verify(x => x.ParseTimeAsync(reminderDate, expectedTimezone, It.IsAny<CancellationToken>()), Times.Once);
  }
}
