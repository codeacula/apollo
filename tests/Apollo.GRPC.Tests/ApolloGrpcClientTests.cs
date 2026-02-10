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
    var result = await client.GetDailyPlanAsync(new Domain.People.ValueObjects.PlatformId("u", "1", Domain.Common.Enums.Platform.Discord));

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Empty(result.Value.SuggestedTasks);
    Assert.Equal("No tasks", result.Value.SelectionRationale);
  }
}
