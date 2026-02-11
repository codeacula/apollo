using Apollo.Core;
using Apollo.Domain.Common.Enums;
using Apollo.Domain.People.ValueObjects;
using Apollo.GRPC.Client;
using Apollo.GRPC.Contracts;
using Apollo.GRPC.Interceptors;
using Apollo.GRPC.Service;

using Grpc.Net.Client;

using Microsoft.Extensions.Logging;

using Moq;

namespace Apollo.GRPC.Tests;

public sealed class ApolloGrpcClientManageAccessTests
{
  private readonly Mock<IApolloGrpcService> _mockService;
  private readonly Mock<ILogger<GrpcClientLoggingInterceptor>> _mockLogger;

  public ApolloGrpcClientManageAccessTests()
  {
    _mockService = new Mock<IApolloGrpcService>();
    _mockLogger = new Mock<ILogger<GrpcClientLoggingInterceptor>>();
  }

  private ApolloGrpcClient CreateClient()
  {
    using var channel = GrpcChannel.ForAddress("http://localhost:5000");
    var hostConfig = new GrpcHostConfig
    {
      ApiToken = string.Empty,
      Host = "localhost",
      Port = 5000,
      UseHttps = false,
      ValidateSslCertificate = false
    };
    var interceptor = new GrpcClientLoggingInterceptor(_mockLogger.Object);
    var client = new ApolloGrpcClient(channel, interceptor, hostConfig);

    // Replace internal service via reflection
    var backing = typeof(ApolloGrpcClient).GetField(
      "<ApolloGrpcService>k__BackingField",
      System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
    backing!.SetValue(client, _mockService.Object);

    return client;
  }

  [Fact]
  public async Task GrantAccessAsyncWithValidRequestReturnsOkAsync()
  {
    // Arrange
    var adminPlatformId = new PlatformId("admin", "111", Platform.Discord);
    var targetPlatformId = new PlatformId("user", "222", Platform.Discord);
    var grpcResult = new GrpcResult<string>
    {
      IsSuccess = true,
      Data = "Access granted"
    };

    _ = _mockService.Setup(s => s.GrantAccessAsync(It.IsAny<ManageAccessRequest>()))
      .ReturnsAsync(grpcResult);

    var client = CreateClient();

    // Act
    var result = await client.GrantAccessAsync(adminPlatformId, targetPlatformId);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal("Access granted", result.Value);
  }

  [Fact]
  public async Task GrantAccessAsyncWithPermissionDeniedReturnsFailAsync()
  {
    // Arrange
    var adminPlatformId = new PlatformId("user", "111", Platform.Discord);
    var targetPlatformId = new PlatformId("admin", "222", Platform.Discord);
    var grpcResult = new GrpcResult<string>
    {
      IsSuccess = false,
      Errors = [new GrpcError("Not authorized", "FORBIDDEN")]
    };

    _ = _mockService.Setup(s => s.GrantAccessAsync(It.IsAny<ManageAccessRequest>()))
      .ReturnsAsync(grpcResult);

    var client = CreateClient();

    // Act
    var result = await client.GrantAccessAsync(adminPlatformId, targetPlatformId);

    // Assert
    Assert.True(result.IsFailed);
    Assert.Contains("Not authorized", result.GetErrorMessages());
  }

  [Fact]
  public async Task RevokeAccessAsyncWithValidRequestReturnsOkAsync()
  {
    // Arrange
    var adminPlatformId = new PlatformId("admin", "111", Platform.Discord);
    var targetPlatformId = new PlatformId("user", "222", Platform.Discord);
    var grpcResult = new GrpcResult<string>
    {
      IsSuccess = true,
      Data = "Access revoked"
    };

    _ = _mockService.Setup(s => s.RevokeAccessAsync(It.IsAny<ManageAccessRequest>()))
      .ReturnsAsync(grpcResult);

    var client = CreateClient();

    // Act
    var result = await client.RevokeAccessAsync(adminPlatformId, targetPlatformId);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal("Access revoked", result.Value);
  }

  [Fact]
  public async Task RevokeAccessAsyncWithErrorReturnsFailAsync()
  {
    // Arrange
    var adminPlatformId = new PlatformId("admin", "111", Platform.Discord);
    var targetPlatformId = new PlatformId("user", "222", Platform.Discord);
    var grpcResult = new GrpcResult<string>
    {
      IsSuccess = false,
      Errors = [new GrpcError("User not found", "NOT_FOUND")]
    };

    _ = _mockService.Setup(s => s.RevokeAccessAsync(It.IsAny<ManageAccessRequest>()))
      .ReturnsAsync(grpcResult);

    var client = CreateClient();

    // Act
    var result = await client.RevokeAccessAsync(adminPlatformId, targetPlatformId);

    // Assert
    Assert.True(result.IsFailed);
    Assert.Contains("User not found", result.GetErrorMessages());
  }

  [Fact]
  public async Task GrantAccessAsyncPassesCorrectParametersAsync()
  {
    // Arrange
    var adminPlatformId = new PlatformId("admin", "111", Platform.Discord);
    var targetPlatformId = new PlatformId("user", "222", Platform.Discord);
    var grpcResult = new GrpcResult<string>
    {
      IsSuccess = true,
      Data = "Access granted"
    };

    _ = _mockService.Setup(s => s.GrantAccessAsync(It.IsAny<ManageAccessRequest>()))
      .ReturnsAsync(grpcResult);

    var client = CreateClient();

    // Act
    _ = await client.GrantAccessAsync(adminPlatformId, targetPlatformId);

    // Assert
    _mockService.Verify(
      s => s.GrantAccessAsync(It.Is<ManageAccessRequest>(req =>
        req.AdminPlatformUserId == "111" &&
        req.AdminUsername == "admin" &&
        req.AdminPlatform == Platform.Discord &&
        req.TargetPlatformUserId == "222" &&
        req.TargetUsername == "user" &&
        req.TargetPlatform == Platform.Discord)),
      Times.Once);
  }

  [Fact]
  public async Task RevokeAccessAsyncPassesCorrectParametersAsync()
  {
    // Arrange
    var adminPlatformId = new PlatformId("admin", "111", Platform.Discord);
    var targetPlatformId = new PlatformId("user", "222", Platform.Discord);
    var grpcResult = new GrpcResult<string>
    {
      IsSuccess = true,
      Data = "Access revoked"
    };

    _ = _mockService.Setup(s => s.RevokeAccessAsync(It.IsAny<ManageAccessRequest>()))
      .ReturnsAsync(grpcResult);

    var client = CreateClient();

    // Act
    _ = await client.RevokeAccessAsync(adminPlatformId, targetPlatformId);

    // Assert
    _mockService.Verify(
      s => s.RevokeAccessAsync(It.Is<ManageAccessRequest>(req =>
        req.AdminPlatformUserId == "111" &&
        req.AdminUsername == "admin" &&
        req.AdminPlatform == Platform.Discord &&
        req.TargetPlatformUserId == "222" &&
        req.TargetUsername == "user" &&
        req.TargetPlatform == Platform.Discord)),
      Times.Once);
  }
}
