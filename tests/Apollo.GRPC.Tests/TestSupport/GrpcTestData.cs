using Apollo.Core.Configuration;
using Apollo.Core.People;
using Apollo.Core.ToDos;
using Apollo.Domain.Common.Enums;
using Apollo.Domain.Common.ValueObjects;
using Apollo.Domain.People.Models;
using Apollo.Domain.People.ValueObjects;
using Apollo.Domain.ToDos.Models;
using Apollo.Domain.ToDos.ValueObjects;
using Apollo.GRPC.Context;
using Apollo.GRPC.Contracts;
using Apollo.GRPC.Service;
using Apollo.GRPC.Tests.Interceptors;

using MediatR;

using Microsoft.AspNetCore.Http;

using Moq;

namespace Apollo.GRPC.Tests.TestSupport;

internal static class GrpcTestData
{
  public static Person CreatePerson(
    string username = "user",
    string platformUserId = "1",
    Platform platform = Platform.Discord,
    string? timeZoneId = null,
    PersonId? personId = null)
  {
    PersonTimeZoneId? parsedTimeZoneId = null;
    if (timeZoneId is not null && PersonTimeZoneId.TryParse(timeZoneId, out var tzId, out _))
    {
      parsedTimeZoneId = tzId;
    }

    return new Person
    {
      Id = personId ?? new PersonId(Guid.NewGuid()),
      PlatformId = new PlatformId(username, platformUserId, platform),
      Username = new Username(username),
      HasAccess = new HasAccess(true),
      TimeZoneId = parsedTimeZoneId,
      CreatedOn = new CreatedOn(DateTime.UtcNow),
      UpdatedOn = new UpdatedOn(DateTime.UtcNow)
    };
  }

  public static Reminder CreateReminder(PersonId personId, string details, DateTime reminderTime)
  {
    return new Reminder
    {
      Id = new ReminderId(Guid.NewGuid()),
      PersonId = personId,
      Details = new Details(details),
      ReminderTime = new ReminderTime(reminderTime),
      CreatedOn = new CreatedOn(DateTime.UtcNow),
      UpdatedOn = new UpdatedOn(DateTime.UtcNow)
    };
  }

  public static NewMessageRequest CreateNewMessageRequest(
    string username = "testuser",
    string platformUserId = "123",
    Platform platform = Platform.Discord,
    string content = "Hello")
  {
    return new NewMessageRequest
    {
      Platform = platform,
      PlatformUserId = platformUserId,
      Username = username,
      Content = content
    };
  }

  public static CreateReminderRequest CreateReminderRequest(string reminderTime, string message = "Test")
  {
    return new CreateReminderRequest
    {
      Username = "user",
      PlatformUserId = "1",
      Platform = Platform.Discord,
      Message = message,
      ReminderTime = reminderTime
    };
  }

  public static ApolloGrpcService CreateApolloGrpcService(
    IMediator mediator,
    IUserContext userContext,
    ITimeParsingService timeParsingService)
  {
    return new ApolloGrpcService(
      mediator,
      Mock.Of<IReminderStore>(),
      Mock.Of<IPersonStore>(),
      timeParsingService,
      Mock.Of<IConfigurationStore>(),
      userContext);
  }

  public static TestServerCallContext CreateServerCallContext(IServiceProvider serviceProvider)
  {
    return new TestServerCallContext(new DefaultHttpContext { RequestServices = serviceProvider });
  }
}
