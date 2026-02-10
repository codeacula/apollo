using Apollo.Application.ToDos.Models;
using Apollo.Core.People;
using Apollo.Core.ToDos;
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
      Id = new Domain.People.ValueObjects.PersonId(Guid.NewGuid()),
      PlatformId = new Domain.People.ValueObjects.PlatformId("user", "1", Domain.Common.Enums.Platform.Discord),
      Username = new Domain.People.ValueObjects.Username("user"),
      HasAccess = new Domain.People.ValueObjects.HasAccess(true),
      CreatedOn = new Domain.Common.ValueObjects.CreatedOn(DateTime.UtcNow),
      UpdatedOn = new Domain.Common.ValueObjects.UpdatedOn(DateTime.UtcNow)
    };
    _ = userContext.SetupGet(u => u.Person).Returns(dummyPerson);

    var service = new ApolloGrpcService(
      mediator.Object,
      Mock.Of<IReminderStore>(),
      Mock.Of<IPersonStore>(),
      Mock.Of<IFuzzyTimeParser>(),
      TimeProvider.System,
      new SuperAdminConfig(),
      userContext.Object
    );

    var request = new GetDailyPlanRequest { Username = "user", PlatformUserId = "1", Platform = Domain.Common.Enums.Platform.Discord };

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
}
