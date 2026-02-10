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

public class UserResolutionInterceptorTests
{
  private readonly Mock<IMediator> _mediatorMock;
  private readonly Mock<IUserContext> _userContextMock;
  private readonly Mock<IPersonStore> _personStoreMock;
  private readonly Mock<IServiceProvider> _serviceProviderMock;
  private readonly UserResolutionInterceptor _interceptor;
  private readonly DefaultHttpContext _httpContext;

  public UserResolutionInterceptorTests()
  {
    _mediatorMock = new Mock<IMediator>();
    _userContextMock = new Mock<IUserContext>();
    _personStoreMock = new Mock<IPersonStore>();
    _serviceProviderMock = new Mock<IServiceProvider>();
    _httpContext = new DefaultHttpContext { RequestServices = _serviceProviderMock.Object };

    _interceptor = new UserResolutionInterceptor();

    _ = _serviceProviderMock.Setup(x => x.GetService(typeof(IMediator))).Returns(_mediatorMock.Object);
    _ = _serviceProviderMock.Setup(x => x.GetService(typeof(IUserContext))).Returns(_userContextMock.Object);
    _ = _serviceProviderMock.Setup(x => x.GetService(typeof(IPersonStore))).Returns(_personStoreMock.Object);
  }

  [Fact]
  public async Task InterceptAuthenticatedRequestResolvesUserAsync()
  {
    // Arrange
    var request = new NewMessageRequest
    {
      Platform = Platform.Discord,
      PlatformUserId = "123",
      Username = "testuser",
      Content = "Hello"
    };

    var personId = new PersonId(Guid.NewGuid());
    var person = new Person
    {
      Id = personId,
      PlatformId = new PlatformId("testuser", "123", Platform.Discord),
      Username = new Username("testuser"),
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

    // Assert
    _userContextMock.VerifySet(x => x.Person = person, Times.Once);
    _mediatorMock.Verify(m => m.Send(It.Is<GetOrCreatePersonByPlatformIdQuery>(q =>
        q.PlatformId.PlatformUserId == "123"), It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task InterceptNonAuthenticatedRequestSkipsResolutionAsync()
  {
    // Arrange
    const string request = "NotAuthenticated"; // Just a string, doesn't implement IAuthenticatedRequest
    var context = new TestServerCallContext(_httpContext);
    static Task<string> continuationAsync(string req, ServerCallContext ctx)
    {
      return Task.FromResult("Response");
    }

    // Act
    _ = await _interceptor.UnaryServerHandler(request, context, continuationAsync);

    // Assert
    _userContextMock.VerifySet(x => x.Person = It.IsAny<Person>(), Times.Never);
    _mediatorMock.Verify(m => m.Send(It.IsAny<GetOrCreatePersonByPlatformIdQuery>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task InterceptAuthenticatedRequestRegistersDiscordNotificationChannelAsync()
  {
    // Arrange
    var request = new NewMessageRequest
    {
      Platform = Platform.Discord,
      PlatformUserId = "123",
      Username = "testuser",
      Content = "Hello"
    };

    var personId = new PersonId(Guid.NewGuid());
    var person = new Person
    {
      Id = personId,
      PlatformId = new PlatformId("testuser", "123", Platform.Discord),
      Username = new Username("testuser"),
      HasAccess = new HasAccess(true),
      CreatedOn = new CreatedOn(DateTime.UtcNow),
      UpdatedOn = new UpdatedOn(DateTime.UtcNow)
    };

    _ = _mediatorMock.Setup(m => m.Send(It.IsAny<GetOrCreatePersonByPlatformIdQuery>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Ok(person));

    _ = _personStoreMock.Setup(x => x.EnsureNotificationChannelAsync(
        It.IsAny<Person>(),
        It.IsAny<NotificationChannel>(),
        It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Ok());

    var context = new TestServerCallContext(_httpContext);

    static Task<string> continuationAsync(NewMessageRequest req, ServerCallContext ctx)
    {
      return Task.FromResult("Response");
    }

    // Act
    _ = await _interceptor.UnaryServerHandler(request, context, continuationAsync);

    // Assert
    _personStoreMock.Verify(x => x.EnsureNotificationChannelAsync(
      person,
      It.Is<NotificationChannel>(c =>
        c.Type == NotificationChannelType.Discord &&
        c.Identifier == "123" &&
        c.IsEnabled),
      It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task InterceptAuthenticatedRequestDoesNotRegisterChannelWhenPersonResolutionFailsAsync()
  {
    // Arrange
    var request = new NewMessageRequest
    {
      Platform = Platform.Discord,
      PlatformUserId = "123",
      Username = "testuser",
      Content = "Hello"
    };

    _ = _mediatorMock.Setup(m => m.Send(It.IsAny<GetOrCreatePersonByPlatformIdQuery>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Fail<Person>("Person not found"));

    var context = new TestServerCallContext(_httpContext);

    static Task<string> continuationAsync(NewMessageRequest req, ServerCallContext ctx)
    {
      return Task.FromResult("Response");
    }

    // Act
    _ = await _interceptor.UnaryServerHandler(request, context, continuationAsync);

    // Assert
    _personStoreMock.Verify(x => x.EnsureNotificationChannelAsync(
      It.IsAny<Person>(),
      It.IsAny<NotificationChannel>(),
      It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task InterceptAuthenticatedRequestSkipsChannelRegistrationForNonDiscordPlatformAsync()
  {
    // Arrange
    var request = new NewMessageRequest
    {
      Platform = Platform.Web,
      PlatformUserId = "456",
      Username = "webuser",
      Content = "Hello"
    };

    var personId = new PersonId(Guid.NewGuid());
    var person = new Person
    {
      Id = personId,
      PlatformId = new PlatformId("webuser", "456", Platform.Web),
      Username = new Username("webuser"),
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

    // Assert
    _personStoreMock.Verify(x => x.EnsureNotificationChannelAsync(
      It.IsAny<Person>(),
      It.IsAny<NotificationChannel>(),
      It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task InterceptAuthenticatedRequestContinuesWhenChannelRegistrationFailsAsync()
  {
    // Arrange
    var request = new NewMessageRequest
    {
      Platform = Platform.Discord,
      PlatformUserId = "123",
      Username = "testuser",
      Content = "Hello"
    };

    var personId = new PersonId(Guid.NewGuid());
    var person = new Person
    {
      Id = personId,
      PlatformId = new PlatformId("testuser", "123", Platform.Discord),
      Username = new Username("testuser"),
      HasAccess = new HasAccess(true),
      CreatedOn = new CreatedOn(DateTime.UtcNow),
      UpdatedOn = new UpdatedOn(DateTime.UtcNow)
    };

    _ = _mediatorMock.Setup(m => m.Send(It.IsAny<GetOrCreatePersonByPlatformIdQuery>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Ok(person));

    _ = _personStoreMock.Setup(x => x.EnsureNotificationChannelAsync(
        It.IsAny<Person>(),
        It.IsAny<NotificationChannel>(),
        It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Fail("DB error"));

    var context = new TestServerCallContext(_httpContext);

    static Task<string> continuationAsync(NewMessageRequest req, ServerCallContext ctx)
    {
      return Task.FromResult("Response");
    }

    // Act
    var response = await _interceptor.UnaryServerHandler(request, context, continuationAsync);

    // Assert
    Assert.Equal("Response", response);
    _userContextMock.VerifySet(x => x.Person = person, Times.Once);
  }
}

public class TestServerCallContext : ServerCallContext
{
  private readonly HttpContext _httpContext;
  private readonly Metadata _requestHeaders;
  private readonly CancellationToken _cancellationToken;
  private readonly Metadata _responseTrailers;
  private Status _status;
  private WriteOptions _writeOptions;
  private readonly AuthContext _authContext;
  private readonly IDictionary<object, object> _userState;

  public TestServerCallContext(HttpContext httpContext)
  {
    _httpContext = httpContext;
    _requestHeaders = [];
    _cancellationToken = CancellationToken.None;
    _responseTrailers = [];
    _status = Status.DefaultSuccess;
    _writeOptions = new WriteOptions();
    _authContext = new AuthContext(string.Empty, []);
    _userState = new Dictionary<object, object>
    {
      // This is the magic key used by Grpc.AspNetCore.Server to store HttpContext
      // We might need to adjust this if the key is internal/different
      ["__HttpContext"] = httpContext
    };
  }

  protected override string MethodCore => "Method";
  protected override string HostCore => "Host";
  protected override string PeerCore => "Peer";
  protected override DateTime DeadlineCore => DateTime.MaxValue;
  protected override Metadata RequestHeadersCore => _requestHeaders;
  protected override CancellationToken CancellationTokenCore => _cancellationToken;
  protected override Metadata ResponseTrailersCore => _responseTrailers;
  protected override Status StatusCore { get => _status; set => _status = value; }
  protected override WriteOptions WriteOptionsCore { get => _writeOptions; set => _writeOptions = value; }
  protected override AuthContext AuthContextCore => _authContext;

  protected override ContextPropagationToken CreatePropagationTokenCore(ContextPropagationOptions options) => null;
  protected override Task WriteResponseHeadersAsyncCore(Metadata responseHeaders) => Task.CompletedTask;
  protected override IDictionary<object, object> UserStateCore => _userState;
}
