using Apollo.Core.People;
using Apollo.Domain.Common.Enums;
using Apollo.Domain.Common.ValueObjects;
using Apollo.Domain.People.Models;
using Apollo.Domain.People.ValueObjects;
using Apollo.GRPC.Attributes;
using Apollo.GRPC.Context;
using Apollo.GRPC.Interceptors;

using Grpc.Core;

using Microsoft.AspNetCore.Http;

using Moq;

namespace Apollo.GRPC.Tests.Interceptors;

public class AuthorizationInterceptorTests
{
  private readonly Mock<IUserContext> _userContextMock;
  private readonly Mock<IServiceProvider> _serviceProviderMock;
  private readonly SuperAdminConfig _superAdminConfig;
  private readonly AuthorizationInterceptor _interceptor;
  private readonly DefaultHttpContext _httpContext;

  public AuthorizationInterceptorTests()
  {
    _userContextMock = new Mock<IUserContext>();
    _serviceProviderMock = new Mock<IServiceProvider>();
    _superAdminConfig = new SuperAdminConfig { DiscordUserId = "999" }; // Admin ID

    _httpContext = new DefaultHttpContext { RequestServices = _serviceProviderMock.Object };

    _interceptor = new AuthorizationInterceptor(_superAdminConfig);

    _ = _serviceProviderMock.Setup(x => x.GetService(typeof(IUserContext))).Returns(_userContextMock.Object);
  }

  [Fact]
  public async Task InterceptRequireAccessNoAccessThrowsRpcExceptionAsync()
  {
    // Arrange
    var metadata = new EndpointMetadataCollection(new RequireAccessAttribute());
    var endpoint = new Endpoint(null, metadata, "TestEndpoint");
    _httpContext.SetEndpoint(endpoint);

    var person = CreatePerson(hasAccess: false);
    _ = _userContextMock.Setup(x => x.Person).Returns(person);

    var context = new TestServerCallContext(_httpContext);
    static Task<string> continuationAsync(string req, ServerCallContext ctx)
    {
      return Task.FromResult("Response");
    }

    // Act & Assert
    var ex = await Assert.ThrowsAsync<RpcException>(() =>
        _interceptor.UnaryServerHandler("Request", context, continuationAsync));

    Assert.Equal(StatusCode.PermissionDenied, ex.Status.StatusCode);
  }

  [Fact]
  public async Task InterceptRequireSuperAdminIsSuperAdminProceedsAsync()
  {
    // Arrange
    var metadata = new EndpointMetadataCollection(new RequireSuperAdminAttribute());
    var endpoint = new Endpoint(null, metadata, "TestEndpoint");
    _httpContext.SetEndpoint(endpoint);

    // Person matches SuperAdminConfig
    var person = CreatePerson(hasAccess: true, platformUserId: "999", platform: Platform.Discord);
    _ = _userContextMock.Setup(x => x.Person).Returns(person);

    var context = new TestServerCallContext(_httpContext);
    static Task<string> continuationAsync(string req, ServerCallContext ctx)
    {
      return Task.FromResult("Response");
    }

    // Act
    var result = await _interceptor.UnaryServerHandler("Request", context, continuationAsync);

    // Assert
    Assert.Equal("Response", result);
  }

  [Fact]
  public async Task InterceptRequireSuperAdminNotSuperAdminThrowsRpcExceptionAsync()
  {
    // Arrange
    var metadata = new EndpointMetadataCollection(new RequireSuperAdminAttribute());
    var endpoint = new Endpoint(null, metadata, "TestEndpoint");
    _httpContext.SetEndpoint(endpoint);

    // Person does NOT match SuperAdminConfig
    var person = CreatePerson(hasAccess: true, platformUserId: "123", platform: Platform.Discord);
    _ = _userContextMock.Setup(x => x.Person).Returns(person);

    var context = new TestServerCallContext(_httpContext);
    static Task<string> continuationAsync(string req, ServerCallContext ctx)
    {
      return Task.FromResult("Response");
    }

    // Act & Assert
    var ex = await Assert.ThrowsAsync<RpcException>(() =>
        _interceptor.UnaryServerHandler("Request", context, continuationAsync));

    Assert.Equal(StatusCode.PermissionDenied, ex.Status.StatusCode);
  }

  [Fact]
  public async Task InterceptNoAttributeProceedsAsync()
  {
    // Arrange
    var metadata = new EndpointMetadataCollection(); // Empty
    var endpoint = new Endpoint(null, metadata, "TestEndpoint");
    _httpContext.SetEndpoint(endpoint);

    var context = new TestServerCallContext(_httpContext);
    static Task<string> continuationAsync(string req, ServerCallContext ctx)
    {
      return Task.FromResult("Response");
    }

    // Act
    var result = await _interceptor.UnaryServerHandler("Request", context, continuationAsync);

    // Assert
    Assert.Equal("Response", result);
  }

  private static Person CreatePerson(bool hasAccess, string platformUserId = "123", Platform platform = Platform.Discord)
  {
    return new Person
    {
      Id = new PersonId(Guid.NewGuid()),
      PlatformId = new PlatformId("testuser", platformUserId, platform),
      Username = new Username("testuser"),
      HasAccess = new HasAccess(hasAccess),
      CreatedOn = new CreatedOn(DateTime.UtcNow),
      UpdatedOn = new UpdatedOn(DateTime.UtcNow)
    };
  }
}
