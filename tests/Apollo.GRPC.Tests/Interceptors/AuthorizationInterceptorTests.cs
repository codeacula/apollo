using System.Security.Claims;
using Apollo.Core.People;
using Apollo.Domain.Common.Enums;
using Apollo.Domain.Common.ValueObjects;
using Apollo.Domain.People.Models;
using Apollo.Domain.People.ValueObjects;
using Apollo.GRPC.Attributes;
using Apollo.GRPC.Context;
using Apollo.GRPC.Interceptors;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

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

        _serviceProviderMock.Setup(x => x.GetService(typeof(IUserContext))).Returns(_userContextMock.Object);
    }

    [Fact]
    public async Task Intercept_RequireAccess_HasAccess_Proceeds()
    {
        // Arrange
        var metadata = new EndpointMetadataCollection(new RequireAccessAttribute());
        var endpoint = new Endpoint(null, metadata, "TestEndpoint");
        _httpContext.SetEndpoint(endpoint);
        
        var person = CreatePerson(hasAccess: true);
        _userContextMock.Setup(x => x.Person).Returns(person);

        var context = new TestServerCallContext(_httpContext);
        UnaryServerMethod<string, string> continuation = (req, ctx) => Task.FromResult("Response");

        // Act
        var result = await _interceptor.UnaryServerHandler("Request", context, continuation);

        // Assert
        Assert.Equal("Response", result);
    }

    [Fact]
    public async Task Intercept_RequireAccess_NoAccess_ThrowsRpcException()
    {
        // Arrange
        var metadata = new EndpointMetadataCollection(new RequireAccessAttribute());
        var endpoint = new Endpoint(null, metadata, "TestEndpoint");
        _httpContext.SetEndpoint(endpoint);
        
        var person = CreatePerson(hasAccess: false);
        _userContextMock.Setup(x => x.Person).Returns(person);

        var context = new TestServerCallContext(_httpContext);
        UnaryServerMethod<string, string> continuation = (req, ctx) => Task.FromResult("Response");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<RpcException>(() => 
            _interceptor.UnaryServerHandler("Request", context, continuation));
        
        Assert.Equal(StatusCode.PermissionDenied, ex.Status.StatusCode);
    }

    [Fact]
    public async Task Intercept_RequireSuperAdmin_IsSuperAdmin_Proceeds()
    {
        // Arrange
        var metadata = new EndpointMetadataCollection(new RequireSuperAdminAttribute());
        var endpoint = new Endpoint(null, metadata, "TestEndpoint");
        _httpContext.SetEndpoint(endpoint);
        
        // Person matches SuperAdminConfig
        var person = CreatePerson(hasAccess: true, platformUserId: "999", platform: Platform.Discord);
        _userContextMock.Setup(x => x.Person).Returns(person);

        var context = new TestServerCallContext(_httpContext);
        UnaryServerMethod<string, string> continuation = (req, ctx) => Task.FromResult("Response");

        // Act
        var result = await _interceptor.UnaryServerHandler("Request", context, continuation);

        // Assert
        Assert.Equal("Response", result);
    }

    [Fact]
    public async Task Intercept_RequireSuperAdmin_NotSuperAdmin_ThrowsRpcException()
    {
        // Arrange
        var metadata = new EndpointMetadataCollection(new RequireSuperAdminAttribute());
        var endpoint = new Endpoint(null, metadata, "TestEndpoint");
        _httpContext.SetEndpoint(endpoint);
        
        // Person does NOT match SuperAdminConfig
        var person = CreatePerson(hasAccess: true, platformUserId: "123", platform: Platform.Discord);
        _userContextMock.Setup(x => x.Person).Returns(person);

        var context = new TestServerCallContext(_httpContext);
        UnaryServerMethod<string, string> continuation = (req, ctx) => Task.FromResult("Response");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<RpcException>(() => 
            _interceptor.UnaryServerHandler("Request", context, continuation));
        
        Assert.Equal(StatusCode.PermissionDenied, ex.Status.StatusCode);
    }

    [Fact]
    public async Task Intercept_NoAttribute_Proceeds()
    {
        // Arrange
        var metadata = new EndpointMetadataCollection(); // Empty
        var endpoint = new Endpoint(null, metadata, "TestEndpoint");
        _httpContext.SetEndpoint(endpoint);
        
        var context = new TestServerCallContext(_httpContext);
        UnaryServerMethod<string, string> continuation = (req, ctx) => Task.FromResult("Response");

        // Act
        var result = await _interceptor.UnaryServerHandler("Request", context, continuation);

        // Assert
        Assert.Equal("Response", result);
    }
    
    private Person CreatePerson(bool hasAccess, string platformUserId = "123", Platform platform = Platform.Discord)
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
