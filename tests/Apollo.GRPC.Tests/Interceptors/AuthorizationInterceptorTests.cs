using Apollo.Core.Configuration;
using Apollo.Domain.Common.Enums;
using Apollo.Domain.People.Models;
using Apollo.Domain.People.ValueObjects;
using Apollo.GRPC.Attributes;
using Apollo.GRPC.Context;
using Apollo.GRPC.Interceptors;
using Apollo.GRPC.Tests.TestSupport;

using FluentResults;

using Grpc.Core;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

using Moq;

namespace Apollo.GRPC.Tests.Interceptors;

public class AuthorizationInterceptorTests
{
  private readonly Mock<IUserContext> _userContextMock;
  private readonly Mock<IServiceProvider> _serviceProviderMock;
  private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
  private readonly Mock<IConfigurationStore> _configurationStoreMock;
  private readonly AuthorizationInterceptor _interceptor;
  private readonly DefaultHttpContext _httpContext;

  public AuthorizationInterceptorTests()
  {
    _userContextMock = new Mock<IUserContext>();
    _serviceProviderMock = new Mock<IServiceProvider>();
    _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
    _configurationStoreMock = new Mock<IConfigurationStore>();

    var configData = new ConfigurationData { SuperAdminDiscordUserId = "999" }; // Admin ID
    _ = _configurationStoreMock.Setup(c => c.GetAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(configData));

    var scopeMock = new Mock<IServiceScope>();
    _ = scopeMock.Setup(s => s.ServiceProvider).Returns(_serviceProviderMock.Object);
    _ = _serviceScopeFactoryMock.Setup(f => f.CreateScope()).Returns(scopeMock.Object);

    _httpContext = new DefaultHttpContext { RequestServices = _serviceProviderMock.Object };

    _interceptor = new AuthorizationInterceptor(_serviceScopeFactoryMock.Object);

    _ = _serviceProviderMock.Setup(x => x.GetService(typeof(IUserContext))).Returns(_userContextMock.Object);
    _ = _serviceProviderMock.Setup(x => x.GetService(typeof(IConfigurationStore))).Returns(_configurationStoreMock.Object);
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

    var context = CreateContext(new RequireAccessAttribute());

    // Act & Assert
    var ex = await Assert.ThrowsAsync<RpcException>(() =>
        _interceptor.UnaryServerHandler("Request", context, ContinuationAsync));

    Assert.Equal(StatusCode.PermissionDenied, ex.Status.StatusCode);
  }

  [Fact]
  public async Task InterceptRequireSuperAdminIsSuperAdminProceedsAsync()
  {
    // Arrange
    var person = CreatePerson(hasAccess: true, platformUserId: "999", platform: Platform.Discord);
    _ = _userContextMock.Setup(x => x.Person).Returns(person);

    var context = CreateContext(new RequireSuperAdminAttribute());

    // Act
    var result = await _interceptor.UnaryServerHandler("Request", context, ContinuationAsync);

    // Assert
    Assert.Equal("Response", result);
  }

  [Fact]
  public async Task InterceptRequireSuperAdminNotSuperAdminThrowsRpcExceptionAsync()
  {
    // Arrange
    var person = CreatePerson(hasAccess: true, platformUserId: "123", platform: Platform.Discord);
    _ = _userContextMock.Setup(x => x.Person).Returns(person);

    var context = CreateContext(new RequireSuperAdminAttribute());

    // Act & Assert
    var ex = await Assert.ThrowsAsync<RpcException>(() =>
        _interceptor.UnaryServerHandler("Request", context, ContinuationAsync));

    Assert.Equal(StatusCode.PermissionDenied, ex.Status.StatusCode);
  }

  [Fact]
  public async Task InterceptNoAttributeProceedsAsync()
  {
    // Arrange
    var context = CreateContext();

    // Act
    var result = await _interceptor.UnaryServerHandler("Request", context, ContinuationAsync);

    // Assert
    Assert.Equal("Response", result);
  }

  private static Person CreatePerson(bool hasAccess, string platformUserId = "123", Platform platform = Platform.Discord)
  {
    return GrpcTestData.CreatePerson(platformUserId: platformUserId, platform: platform) with
    {
      HasAccess = new HasAccess(hasAccess)
    };
  }

  private static Task<string> ContinuationAsync(string _, ServerCallContext __) => Task.FromResult("Response");

  private TestServerCallContext CreateContext(params object[] metadataItems)
  {
    var metadata = new EndpointMetadataCollection(metadataItems);
    var endpoint = new Endpoint(null, metadata, "TestEndpoint");
    _httpContext.SetEndpoint(endpoint);
    return new TestServerCallContext(_httpContext);
  }
}
