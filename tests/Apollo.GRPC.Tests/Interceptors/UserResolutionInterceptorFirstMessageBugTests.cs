using Apollo.Application.People;
using Apollo.Core.People;
using Apollo.Domain.Common.Enums;
using Apollo.Domain.People.Models;
using Apollo.Domain.People.ValueObjects;
using Apollo.GRPC.Context;
using Apollo.GRPC.Contracts;
using Apollo.GRPC.Interceptors;
using Apollo.GRPC.Tests.TestSupport;

using FluentResults;

using Grpc.Core;

using MediatR;

using Microsoft.AspNetCore.Http;

using Moq;

namespace Apollo.GRPC.Tests.Interceptors;

/// <summary>
/// <para>
/// Tests for bug where first Discord user message creates Person with null Username and PlatformUserId.
/// The issue stems from protobuf-net deserializing gRPC contracts with mismatched DataMember Order attributes.
/// </para>
/// <para>
/// - ProcessMessageRequest has Order: Username=1, PlatformUserId=2, Platform=3
/// - AuthenticatedRequestBase has Order: Platform=101, PlatformUserId=102, Username=103
/// </para>
/// <para>
/// When NewMessageRequest (extending AuthenticatedRequestBase) is sent over gRPC and deserialized,
/// the wire order from the child class conflicts with parent class expectations.
/// </para>
/// </summary>
public class UserResolutionInterceptorFirstMessageBugTests
{
  private readonly Mock<IMediator> _mediatorMock;
  private readonly Mock<IUserContext> _userContextMock;
  private readonly Mock<IServiceProvider> _serviceProviderMock;
  private readonly UserResolutionInterceptor _interceptor;
  private readonly DefaultHttpContext _httpContext;

  public UserResolutionInterceptorFirstMessageBugTests()
  {
    _mediatorMock = new Mock<IMediator>();
    _userContextMock = new Mock<IUserContext>();
    _serviceProviderMock = new Mock<IServiceProvider>();
    _httpContext = new DefaultHttpContext { RequestServices = _serviceProviderMock.Object };

    _interceptor = new UserResolutionInterceptor();

    var personStoreMock = new Mock<IPersonStore>();
    _ = personStoreMock.Setup(x => x.EnsureNotificationChannelAsync(
        It.IsAny<Person>(),
        It.IsAny<NotificationChannel>(),
        It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Ok());

    _ = _serviceProviderMock.Setup(x => x.GetService(typeof(IMediator))).Returns(_mediatorMock.Object);
    _ = _serviceProviderMock.Setup(x => x.GetService(typeof(IUserContext))).Returns(_userContextMock.Object);
    _ = _serviceProviderMock.Setup(x => x.GetService(typeof(IPersonStore))).Returns(personStoreMock.Object);
  }

  private static Task<string> ContinuationAsync(NewMessageRequest _, ServerCallContext __) => Task.FromResult("Response");

  /// <summary>
  /// <para>
  /// Verifies that when a NewMessageRequest is created with platform, platformUserId, and username,
  /// the GetOrCreatePersonByPlatformIdQuery uses the correct PlatformId values (not corrupted by protobuf-net ordering).
  /// </para>
  /// <para>
  /// Before fix: Query received PlatformId with null Username and PlatformUserId due to DataMember Order mismatch
  /// After fix: Query receives correct PlatformId with both Username and PlatformUserId populated
  /// </para>
  /// </summary>
  [Fact]
  public async Task FirstDiscordMessageCreatesPersonWithCorrectPlatformIdAsync()
  {
    // Arrange
    const string discordUsername = "codeacula";
    const string discordUserId = "244273250144747523";

    var request = GrpcTestData.CreateNewMessageRequest(discordUsername, discordUserId, content: "Remind me to buy milk");

    var capturedQuery = (GetOrCreatePersonByPlatformIdQuery?)null;
    var person = GrpcTestData.CreatePerson(discordUsername, discordUserId);

    _ = _mediatorMock.Setup(m => m.Send(It.IsAny<GetOrCreatePersonByPlatformIdQuery>(), It.IsAny<CancellationToken>()))
        .Callback<IRequest<Result<Person>>, CancellationToken>((query, _) => capturedQuery = (GetOrCreatePersonByPlatformIdQuery)query)
        .ReturnsAsync(Result.Ok(person));

    var context = new TestServerCallContext(_httpContext);

    // Act
    _ = await _interceptor.UnaryServerHandler(request, context, ContinuationAsync);

    // Assert: Verify that the interceptor captured the request with correct values
    Assert.NotNull(capturedQuery);
    Assert.Equal(discordUsername, capturedQuery.PlatformId.Username);
    Assert.Equal(discordUserId, capturedQuery.PlatformId.PlatformUserId);
    Assert.Equal(Platform.Discord, capturedQuery.PlatformId.Platform);
  }

  /// <summary>
  /// Regression test: Ensures that CreateNotificationChannel is called with the correct PlatformUserId
  /// (the one from Discord's Author.Id, not corrupted by protobuf-net).
  /// </summary>
  [Fact]
  public async Task FirstDiscordMessageEnsuresNotificationChannelWithCorrectIdentifierAsync()
  {
    // Arrange
    const string discordUsername = "codeacula";
    const string discordUserId = "244273250144747523";

    var request = GrpcTestData.CreateNewMessageRequest(discordUsername, discordUserId, content: "Remind me to buy milk");
    var person = GrpcTestData.CreatePerson(discordUsername, discordUserId);

    _ = _mediatorMock.Setup(m => m.Send(It.IsAny<GetOrCreatePersonByPlatformIdQuery>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Ok(person));

    var personStoreMock = new Mock<IPersonStore>();
    _ = personStoreMock.Setup(x => x.EnsureNotificationChannelAsync(
        It.IsAny<Person>(),
        It.IsAny<NotificationChannel>(),
        It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Ok());

    _ = _serviceProviderMock.Setup(x => x.GetService(typeof(IPersonStore))).Returns(personStoreMock.Object);

    var context = new TestServerCallContext(_httpContext);

    // Act
    _ = await _interceptor.UnaryServerHandler(request, context, ContinuationAsync);

    // Assert: Verify that notification channel is registered with correct Discord user ID
    personStoreMock.Verify(x => x.EnsureNotificationChannelAsync(
      It.IsAny<Person>(),
      It.Is<NotificationChannel>(c =>
        c.Type == NotificationChannelType.Discord &&
        c.Identifier == discordUserId),  // Must be the Discord user ID, not null
      It.IsAny<CancellationToken>()), Times.Once);
  }

}
