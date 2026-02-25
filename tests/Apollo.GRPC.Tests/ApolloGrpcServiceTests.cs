using Apollo.Application.ToDos;
using Apollo.Application.ToDos.Models;
using Apollo.Core.People;
using Apollo.Core.ToDos;
using Apollo.Domain.Common.Enums;
using Apollo.Domain.Common.ValueObjects;
using Apollo.Domain.People.ValueObjects;
using Apollo.Domain.ToDos.Models;
using Apollo.Domain.ToDos.ValueObjects;
using Apollo.GRPC.Context;
using Apollo.GRPC.Contracts;
using Apollo.GRPC.Service;

using FluentResults;

using MediatR;

using Moq;

namespace Apollo.GRPC.Tests;

public sealed class ApolloGrpcServiceTests
{
  [Fact]
  public async Task GetDailyPlanAsyncPlanHasNullSuggestedTasksReturnsDtoWithEmptyArrayAsync()
  {
    // Arrange
    var mediator = new Mock<IMediator>();
    var userContext = new Mock<IUserContext>();

    // Return a DailyPlan with null SuggestedTasks from the mediator
    _ = mediator.Setup(m => m.Send(It.IsAny<IRequest<Result<DailyPlan>>>(), default))
      .ReturnsAsync(Result.Ok(new DailyPlan(null!, "Rationale", 0)));

    // Provide a dummy Person on the user context so the service doesn't NRE
    var dummyPerson = new Domain.People.Models.Person
    {
      Id = new PersonId(Guid.NewGuid()),
      PlatformId = new PlatformId("user", "1", Platform.Discord),
      Username = new Username("user"),
      HasAccess = new HasAccess(true),
      CreatedOn = new CreatedOn(DateTime.UtcNow),
      UpdatedOn = new UpdatedOn(DateTime.UtcNow)
    };
    _ = userContext.SetupGet(u => u.Person).Returns(dummyPerson);

    var service = new ApolloGrpcService(
      mediator.Object,
      Mock.Of<IReminderStore>(),
      Mock.Of<IPersonStore>(),
      Mock.Of<ITimeParsingService>(),
      new SuperAdminConfig(),
      userContext.Object
    );

    var request = new GetDailyPlanRequest { Username = "user", PlatformUserId = "1", Platform = Platform.Discord };

    // Act
    var result = await service.GetDailyPlanAsync(request);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Data);
    Assert.NotNull(result.Data.SuggestedTasks);
    Assert.Empty(result.Data.SuggestedTasks);
    Assert.Equal("Rationale", result.Data.SelectionRationale);
    Assert.Equal(0, result.Data.TotalActiveTodos);
  }

  [Fact]
  public async Task CreateReminderAsyncPassesUserTimezoneToTimeParsingServiceAsync()
  {
    // Arrange
    var mediator = new Mock<IMediator>();
    var userContext = new Mock<IUserContext>();
    var timeParsingService = new Mock<ITimeParsingService>();
    var reminderUtc = new DateTime(2026, 3, 1, 21, 0, 0, DateTimeKind.Utc);

    _ = PersonTimeZoneId.TryParse("Europe/London", out var tzId, out _);
    var person = new Domain.People.Models.Person
    {
      Id = new PersonId(Guid.NewGuid()),
      PlatformId = new PlatformId("user", "1", Platform.Discord),
      Username = new Username("user"),
      HasAccess = new HasAccess(true),
      TimeZoneId = tzId,
      CreatedOn = new CreatedOn(DateTime.UtcNow),
      UpdatedOn = new UpdatedOn(DateTime.UtcNow)
    };
    _ = userContext.SetupGet(u => u.Person).Returns(person);

    _ = timeParsingService.Setup(t => t.ParseTimeAsync("tomorrow at 3pm", "Europe/London", It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(reminderUtc));

    _ = mediator.Setup(m => m.Send(It.IsAny<CreateReminderCommand>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(new Reminder
      {
        Id = new ReminderId(Guid.NewGuid()),
        PersonId = person.Id,
        Details = new Details("Test"),
        ReminderTime = new ReminderTime(reminderUtc),
        CreatedOn = new CreatedOn(DateTime.UtcNow),
        UpdatedOn = new UpdatedOn(DateTime.UtcNow)
      }));

    var service = new ApolloGrpcService(
      mediator.Object,
      Mock.Of<IReminderStore>(),
      Mock.Of<IPersonStore>(),
      timeParsingService.Object,
      new SuperAdminConfig(),
      userContext.Object
    );

    var request = new CreateReminderRequest
    {
      Username = "user",
      PlatformUserId = "1",
      Platform = Platform.Discord,
      Message = "Test",
      ReminderTime = "tomorrow at 3pm"
    };

    // Act
    var result = await service.CreateReminderAsync(request);

    // Assert — should pass person's timezone to ParseTimeAsync
    Assert.True(result.IsSuccess);
    timeParsingService.Verify(t => t.ParseTimeAsync("tomorrow at 3pm", "Europe/London", It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task CreateReminderAsyncWithNoTimezonePassesNullToTimeParsingServiceAsync()
  {
    // Arrange
    var mediator = new Mock<IMediator>();
    var userContext = new Mock<IUserContext>();
    var timeParsingService = new Mock<ITimeParsingService>();
    var reminderUtc = new DateTime(2026, 3, 1, 15, 0, 0, DateTimeKind.Utc);

    // Person without timezone
    var person = new Domain.People.Models.Person
    {
      Id = new PersonId(Guid.NewGuid()),
      PlatformId = new PlatformId("user", "1", Platform.Discord),
      Username = new Username("user"),
      HasAccess = new HasAccess(true),
      TimeZoneId = null,
      CreatedOn = new CreatedOn(DateTime.UtcNow),
      UpdatedOn = new UpdatedOn(DateTime.UtcNow)
    };
    _ = userContext.SetupGet(u => u.Person).Returns(person);

    _ = timeParsingService.Setup(t => t.ParseTimeAsync("in 10 minutes", null, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(reminderUtc));

    _ = mediator.Setup(m => m.Send(It.IsAny<CreateReminderCommand>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(new Reminder
      {
        Id = new ReminderId(Guid.NewGuid()),
        PersonId = person.Id,
        Details = new Details("Test"),
        ReminderTime = new ReminderTime(reminderUtc),
        CreatedOn = new CreatedOn(DateTime.UtcNow),
        UpdatedOn = new UpdatedOn(DateTime.UtcNow)
      }));

    var service = new ApolloGrpcService(
      mediator.Object,
      Mock.Of<IReminderStore>(),
      Mock.Of<IPersonStore>(),
      timeParsingService.Object,
      new SuperAdminConfig(),
      userContext.Object
    );

    var request = new CreateReminderRequest
    {
      Username = "user",
      PlatformUserId = "1",
      Platform = Platform.Discord,
      Message = "Test",
      ReminderTime = "in 10 minutes"
    };

    // Act
    var result = await service.CreateReminderAsync(request);

    // Assert — should pass null timezone since person has none
    Assert.True(result.IsSuccess);
    timeParsingService.Verify(t => t.ParseTimeAsync("in 10 minutes", null, It.IsAny<CancellationToken>()), Times.Once);
  }
}
