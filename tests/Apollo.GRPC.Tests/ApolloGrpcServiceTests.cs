using Apollo.Application.ToDos;
using Apollo.Application.ToDos.Models;
using Apollo.Core.ToDos;
using Apollo.Domain.Common.Enums;
using Apollo.GRPC.Context;
using Apollo.GRPC.Contracts;
using Apollo.GRPC.Tests.TestSupport;

using FluentResults;

using MediatR;

using Moq;

namespace Apollo.GRPC.Tests;

public sealed class ApolloGrpcServiceTests
{
  public static TheoryData<string?, string, string?> ReminderTimezoneCases => new()
  {
    { "Europe/London", "tomorrow at 3pm", "Europe/London" },
    { null, "in 10 minutes", null }
  };

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
    var dummyPerson = GrpcTestData.CreatePerson();
    _ = userContext.SetupGet(u => u.Person).Returns(dummyPerson);

    var service = GrpcTestData.CreateApolloGrpcService(mediator.Object, userContext.Object, Mock.Of<ITimeParsingService>());

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

  [Theory]
  [MemberData(nameof(ReminderTimezoneCases))]
  public async Task CreateReminderAsyncPassesExpectedTimezoneToTimeParsingServiceAsync(
    string? personTimeZoneId,
    string reminderTime,
    string? expectedTimezone)
  {
    // Arrange
    var mediator = new Mock<IMediator>();
    var userContext = new Mock<IUserContext>();
    var timeParsingService = new Mock<ITimeParsingService>();
    var reminderUtc = new DateTime(2026, 3, 1, 21, 0, 0, DateTimeKind.Utc);
    var person = GrpcTestData.CreatePerson(timeZoneId: personTimeZoneId);

    _ = userContext.SetupGet(u => u.Person).Returns(person);

    _ = timeParsingService.Setup(t => t.ParseTimeAsync(reminderTime, expectedTimezone, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(reminderUtc));

    _ = mediator.Setup(m => m.Send(It.IsAny<CreateReminderCommand>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(GrpcTestData.CreateReminder(person.Id, "Test", reminderUtc)));

    var service = GrpcTestData.CreateApolloGrpcService(mediator.Object, userContext.Object, timeParsingService.Object);
    var request = GrpcTestData.CreateReminderRequest(reminderTime);

    // Act
    var result = await service.CreateReminderAsync(request);

    Assert.True(result.IsSuccess);
    timeParsingService.Verify(t => t.ParseTimeAsync(reminderTime, expectedTimezone, It.IsAny<CancellationToken>()), Times.Once);
  }
}
