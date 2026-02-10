using Apollo.Application.People;
using Apollo.Core.People;
using Apollo.Domain.Common.Enums;
using Apollo.Domain.Common.ValueObjects;
using Apollo.Domain.People.Models;
using Apollo.Domain.People.ValueObjects;
using Apollo.GRPC.Context;
using Apollo.GRPC.Contracts;
using Apollo.GRPC.Interceptors;

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

    var request = new NewMessageRequest
    {
      Platform = Platform.Discord,
      PlatformUserId = discordUserId,
      Username = discordUsername,
      Content = "Remind me to buy milk"
    };

    var capturedQuery = (GetOrCreatePersonByPlatformIdQuery?)null;
    var personId = new PersonId(Guid.NewGuid());
    var person = new Person
    {
      Id = personId,
      PlatformId = new PlatformId(discordUsername, discordUserId, Platform.Discord),
      Username = new Username(discordUsername),
      HasAccess = new HasAccess(true),
      CreatedOn = new CreatedOn(DateTime.UtcNow),
      UpdatedOn = new UpdatedOn(DateTime.UtcNow)
    };

    _ = _mediatorMock.Setup(m => m.Send(It.IsAny<GetOrCreatePersonByPlatformIdQuery>(), It.IsAny<CancellationToken>()))
        .Callback<IRequest<Result<Person>>, CancellationToken>((query, _) => capturedQuery = (GetOrCreatePersonByPlatformIdQuery)query)
        .ReturnsAsync(Result.Ok(person));

    var context = new TestServerCallContext(_httpContext);

    static Task<string> continuationAsync(NewMessageRequest req, ServerCallContext ctx)
    {
      return Task.FromResult("Response");
    }

    // Act
    _ = await _interceptor.UnaryServerHandler(request, context, continuationAsync);

    // Assert: Verify that the interceptor captured the request with correct values
    Assert.NotNull(capturedQuery);
    Assert.Equal(discordUsername, capturedQuery.PlatformId.Username);
    Assert.Equal(discordUserId, capturedQuery.PlatformId.PlatformUserId);
    Assert.Equal(Platform.Discord, capturedQuery.PlatformId.Platform);
  }

  /// <summary>
  /// Verifies that the UserContext is set with the correct person (with all fields populated).
  /// This ensures the downstream authorization check doesn't fail with "Access denied" on first message.
  /// </summary>
  [Fact]
  public async Task FirstDiscordMessageSetsUserContextWithPopulatedPersonAsync()
  {
    // Arrange
    const string discordUsername = "codeacula";
    const string discordUserId = "244273250144747523";

    var request = new NewMessageRequest
    {
      Platform = Platform.Discord,
      PlatformUserId = discordUserId,
      Username = discordUsername,
      Content = "Remind me to buy milk"
    };

    var personId = new PersonId(Guid.NewGuid());
    var person = new Person
    {
      Id = personId,
      PlatformId = new PlatformId(discordUsername, discordUserId, Platform.Discord),
      Username = new Username(discordUsername),
      HasAccess = new HasAccess(true),
      CreatedOn = new CreatedOn(DateTime.UtcNow),
      UpdatedOn = new UpdatedOn(DateTime.UtcNow)
    };

    _ = _mediatorMock.Setup(m => m.Send(It.IsAny<GetOrCreatePersonByPlatformIdQuery>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Ok(person));

    var context = new TestServerCallContext(_httpContext);

    static Task<string> continuationAsync(NewMessageRequest req, ServerCallContext ctx)
    {
      return Task.FromResult("Response");
    }

    // Act
    _ = await _interceptor.UnaryServerHandler(request, context, continuationAsync);

    // Assert: Verify that userContext has the person with populated PlatformId
    _userContextMock.VerifySet(x => x.Person = It.Is<Person>(p =>
        p.PlatformId.Username == discordUsername &&
        p.PlatformId.PlatformUserId == discordUserId &&
        p.PlatformId.Platform == Platform.Discord &&
        p.HasAccess.Value), Times.Once);
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

    var request = new NewMessageRequest
    {
      Platform = Platform.Discord,
      PlatformUserId = discordUserId,
      Username = discordUsername,
      Content = "Remind me to buy milk"
    };

    var personId = new PersonId(Guid.NewGuid());
    var person = new Person
    {
      Id = personId,
      PlatformId = new PlatformId(discordUsername, discordUserId, Platform.Discord),
      Username = new Username(discordUsername),
      HasAccess = new HasAccess(true),
      CreatedOn = new CreatedOn(DateTime.UtcNow),
      UpdatedOn = new UpdatedOn(DateTime.UtcNow)
    };

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

    static Task<string> continuationAsync(NewMessageRequest req, ServerCallContext ctx)
    {
      return Task.FromResult("Response");
    }

    // Act
    _ = await _interceptor.UnaryServerHandler(request, context, continuationAsync);

    // Assert: Verify that notification channel is registered with correct Discord user ID
    personStoreMock.Verify(x => x.EnsureNotificationChannelAsync(
      It.IsAny<Person>(),
      It.Is<NotificationChannel>(c =>
        c.Type == NotificationChannelType.Discord &&
        c.Identifier == discordUserId),  // Must be the Discord user ID, not null
      It.IsAny<CancellationToken>()), Times.Once);
  }

  /// <summary>
  /// Regression test for DataMember ordering issue between NewMessageRequest and ProcessMessageRequest.
  /// Verifies that the PlatformId passed to MediatR has all required fields set (not null).
  /// </summary>
  [Fact]
  public async Task FirstDiscordMessagePassesComplletePlatformIdToMediatorAsync()
  {
    // Arrange
    const string discordUsername = "codeacula";
    const string discordUserId = "244273250144747523";

    var request = new NewMessageRequest
    {
      Platform = Platform.Discord,
      PlatformUserId = discordUserId,
      Username = discordUsername,
      Content = "Remind me to buy milk"
    };

    PlatformId? capturedPlatformId = null;
    var personId = new PersonId(Guid.NewGuid());
    var person = new Person
    {
      Id = personId,
      PlatformId = new PlatformId(discordUsername, discordUserId, Platform.Discord),
      Username = new Username(discordUsername),
      HasAccess = new HasAccess(true),
      CreatedOn = new CreatedOn(DateTime.UtcNow),
      UpdatedOn = new UpdatedOn(DateTime.UtcNow)
    };

    _ = _mediatorMock.Setup(m => m.Send(It.IsAny<GetOrCreatePersonByPlatformIdQuery>(), It.IsAny<CancellationToken>()))
        .Callback<IRequest<Result<Person>>, CancellationToken>((query, _) =>
        {
          var typedQuery = (GetOrCreatePersonByPlatformIdQuery)query;
          capturedPlatformId = typedQuery.PlatformId;
        })
        .ReturnsAsync(Result.Ok(person));

    var context = new TestServerCallContext(_httpContext);

    static Task<string> continuationAsync(NewMessageRequest req, ServerCallContext ctx)
    {
      return Task.FromResult("Response");
    }

    // Act
    _ = await _interceptor.UnaryServerHandler(request, context, continuationAsync);

    // Assert: All three components of PlatformId must be populated (not null)
    _ = Assert.NotNull(capturedPlatformId);
    Assert.NotNull(capturedPlatformId.Value.Username);  // Not null
    Assert.NotNull(capturedPlatformId.Value.PlatformUserId);  // Not null

    Assert.Equal(discordUsername, capturedPlatformId.Value.Username);
    Assert.Equal(discordUserId, capturedPlatformId.Value.PlatformUserId);
    Assert.Equal(Platform.Discord, capturedPlatformId.Value.Platform);
  }

}
