using Apollo.Core;
using Apollo.Domain.Common.Enums;
using Apollo.Domain.People.ValueObjects;
using Apollo.GRPC.Client;
using Apollo.GRPC.Contracts;
using Apollo.GRPC.Interceptors;
using Apollo.GRPC.Service;

using Grpc.Net.Client;

using Moq;

namespace Apollo.GRPC.Tests;

public sealed class ApolloGrpcClientTests
{
  [Fact]
  public async Task SetToDoEnergyAsyncWithValidRequestReturnsOkAsync()
  {
    // Arrange
    var mockService = new Mock<IApolloGrpcService>();
    var toDoId = Guid.NewGuid();
    var grpcResult = new GrpcResult<string>
    {
      IsSuccess = true,
      Data = "Energy updated successfully"
    };

    _ = mockService.Setup(s => s.SetToDoEnergyAsync(It.IsAny<SetToDoEnergyRequest>()))
      .ReturnsAsync(grpcResult);

    using var channel = GrpcChannel.ForAddress("http://localhost:5000");
    var hostConfig = new GrpcHostConfig { ApiToken = string.Empty, Host = "localhost", Port = 5000, UseHttps = false, ValidateSslCertificate = false };
    var interceptor = new GrpcClientLoggingInterceptor(Mock.Of<Microsoft.Extensions.Logging.ILogger<GrpcClientLoggingInterceptor>>());
    var client = new ApolloGrpcClient(channel, interceptor, hostConfig);

    // Replace internal service via reflection
    var backing = typeof(ApolloGrpcClient).GetField("<ApolloGrpcService>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
    backing!.SetValue(client, mockService.Object);

    var platformId = new PlatformId("testuser", "123456", Platform.Discord);

    // Act
    var result = await client.SetToDoEnergyAsync(platformId, toDoId, Level.Yellow);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal("Energy updated successfully", result.Value);

    // Verify the gRPC service was called with correct values
    mockService.Verify(
      s => s.SetToDoEnergyAsync(It.Is<SetToDoEnergyRequest>(req =>
        req.Username == "testuser" &&
        req.PlatformUserId == "123456" &&
        req.Platform == Platform.Discord &&
        req.ToDoId == toDoId &&
        req.Energy == Level.Yellow)),
      Times.Once);
  }

  [Fact]
  public async Task SetToDoEnergyAsyncWithFailureReturnsFailResultAsync()
  {
    // Arrange
    var mockService = new Mock<IApolloGrpcService>();
    var toDoId = Guid.NewGuid();
    var grpcResult = new GrpcResult<string>
    {
      IsSuccess = false,
      Errors = [new GrpcError("Permission denied", "FORBIDDEN")]
    };

    _ = mockService.Setup(s => s.SetToDoEnergyAsync(It.IsAny<SetToDoEnergyRequest>()))
      .ReturnsAsync(grpcResult);

    using var channel = GrpcChannel.ForAddress("http://localhost:5000");
    var hostConfig = new GrpcHostConfig { ApiToken = string.Empty, Host = "localhost", Port = 5000, UseHttps = false, ValidateSslCertificate = false };
    var interceptor = new GrpcClientLoggingInterceptor(Mock.Of<Microsoft.Extensions.Logging.ILogger<GrpcClientLoggingInterceptor>>());
    var client = new ApolloGrpcClient(channel, interceptor, hostConfig);

    // Replace internal service via reflection
    var backing = typeof(ApolloGrpcClient).GetField("<ApolloGrpcService>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
    backing!.SetValue(client, mockService.Object);

    var platformId = new PlatformId("testuser", "123456", Platform.Discord);

    // Act
    var result = await client.SetToDoEnergyAsync(platformId, toDoId, Level.Blue);

    // Assert
    Assert.True(result.IsFailed);
    Assert.Contains("Permission denied", result.GetErrorMessages());
  }

  [Fact]
  public async Task GetDailyPlanAsyncGrpcReturnsNullSuggestedTasksReturnsOkWithEmptyListAsync()
  {
    // Arrange
    var mockService = new Mock<IApolloGrpcService>();
    var grpcResult = new GrpcResult<DailyPlanDTO>
    {
      IsSuccess = true,
      Data = new DailyPlanDTO
      {
        SuggestedTasks = null!, // simulate null coming across the wire
        SelectionRationale = "No tasks",
        TotalActiveTodos = 0
      }
    };

    _ = mockService.Setup(s => s.GetDailyPlanAsync(It.IsAny<GetDailyPlanRequest>()))
      .ReturnsAsync(grpcResult);

    // Use a real channel object as it's required by the client constructor but won't be used in this test
    using var channel = GrpcChannel.ForAddress("http://localhost:5000");
    var hostConfig = new GrpcHostConfig { ApiToken = string.Empty, Host = "localhost", Port = 5000, UseHttps = false, ValidateSslCertificate = false };
    // Create a real interceptor instance (it's a sealed class but cheap to construct)
    var interceptor = new GrpcClientLoggingInterceptor(Mock.Of<Microsoft.Extensions.Logging.ILogger<GrpcClientLoggingInterceptor>>());
    var client = new ApolloGrpcClient(channel, interceptor, hostConfig);

    // Replace internal service via reflection (test seam) - set the private backing field
    var backing = typeof(ApolloGrpcClient).GetField("<ApolloGrpcService>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
    backing!.SetValue(client, mockService.Object);

    // Act
    var result = await client.GetDailyPlanAsync(new PlatformId("u", "1", Platform.Discord));

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Empty(result.Value.SuggestedTasks);
    Assert.Equal("No tasks", result.Value.SelectionRationale);
  }
}
