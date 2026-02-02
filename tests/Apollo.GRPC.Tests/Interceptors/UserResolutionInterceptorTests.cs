using System.Security.Claims;
using Apollo.Application.People.Queries;
using Apollo.Domain.Common.Enums;
using Apollo.Domain.Common.ValueObjects;
using Apollo.Domain.People.Models;
using Apollo.Domain.People.ValueObjects;
using Apollo.GRPC.Context;
using Apollo.GRPC.Contracts;
using Apollo.GRPC.Interceptors;
using FluentResults;
using Grpc.Core;
using Grpc.Core.Interceptors;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Apollo.GRPC.Tests.Interceptors;

public class UserResolutionInterceptorTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IUserContext> _userContextMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly UserResolutionInterceptor _interceptor;
    private readonly DefaultHttpContext _httpContext;

    public UserResolutionInterceptorTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _userContextMock = new Mock<IUserContext>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _httpContext = new DefaultHttpContext { RequestServices = _serviceProviderMock.Object };
        
        _interceptor = new UserResolutionInterceptor();

        _serviceProviderMock.Setup(x => x.GetService(typeof(IMediator))).Returns(_mediatorMock.Object);
        _serviceProviderMock.Setup(x => x.GetService(typeof(IUserContext))).Returns(_userContextMock.Object);
    }

    [Fact]
    public async Task Intercept_AuthenticatedRequest_ResolvesUser()
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

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetOrCreatePersonByPlatformIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(person));

        var context = new TestServerCallContext(_httpContext);
        
        UnaryServerMethod<NewMessageRequest, string> continuation = (req, ctx) => Task.FromResult("Response");

        // Act
        await _interceptor.UnaryServerHandler(request, context, continuation);

        // Assert
        _userContextMock.VerifySet(x => x.Person = person, Times.Once);
        _mediatorMock.Verify(m => m.Send(It.Is<GetOrCreatePersonByPlatformIdQuery>(q => 
            q.PlatformId.PlatformUserId == "123"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Intercept_NonAuthenticatedRequest_SkipsResolution()
    {
        // Arrange
        var request = "NotAuthenticated"; // Just a string, doesn't implement IAuthenticatedRequest
        var context = new TestServerCallContext(_httpContext);
        UnaryServerMethod<string, string> continuation = (req, ctx) => Task.FromResult("Response");

        // Act
        await _interceptor.UnaryServerHandler(request, context, continuation);

        // Assert
        _userContextMock.VerifySet(x => x.Person = It.IsAny<Person>(), Times.Never);
        _mediatorMock.Verify(m => m.Send(It.IsAny<GetOrCreatePersonByPlatformIdQuery>(), It.IsAny<CancellationToken>()), Times.Never);
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
    private AuthContext _authContext;
    private IDictionary<object, object> _userState;

    public TestServerCallContext(HttpContext httpContext)
    {
        _httpContext = httpContext;
        _requestHeaders = new Metadata();
        _cancellationToken = CancellationToken.None;
        _responseTrailers = new Metadata();
        _status = Status.DefaultSuccess;
        _writeOptions = new WriteOptions();
        _authContext = new AuthContext(string.Empty, new Dictionary<string, List<AuthProperty>>());
        _userState = new Dictionary<object, object>();
        
        // This is the magic key used by Grpc.AspNetCore.Server to store HttpContext
        // We might need to adjust this if the key is internal/different
        _userState["__HttpContext"] = httpContext;
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
