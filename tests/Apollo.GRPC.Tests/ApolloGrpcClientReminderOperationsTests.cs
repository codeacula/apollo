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

using CoreCreateReminderRequest = Apollo.Core.Reminders.Requests.CreateReminderRequest;
using GrpcCreateReminderRequest = Apollo.GRPC.Contracts.CreateReminderRequest;

namespace Apollo.GRPC.Tests;

public sealed class ApolloGrpcClientReminderOperationsTests
{
  private readonly Mock<IApolloGrpcService> _mockService;
  private readonly Mock<ILogger<GrpcClientLoggingInterceptor>> _mockLogger;

  public ApolloGrpcClientReminderOperationsTests()
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
  public async Task CreateReminderAsyncWithValidRequestReturnsReminderAsync()
  {
    // Arrange
    var platformId = new PlatformId("testuser", "123456", Platform.Discord);
    var createRequest = new CoreCreateReminderRequest
    {
      PlatformId = platformId,
      Message = "Take a break",
      ReminderTime = "in 30 minutes"
    };

    var reminderDto = new ReminderDTO
    {
      Id = Guid.NewGuid(),
      PersonId = Guid.NewGuid(),
      Details = "Take a break",
      ReminderTime = DateTime.UtcNow.AddMinutes(30),
      CreatedOn = DateTime.UtcNow,
      UpdatedOn = DateTime.UtcNow
    };

    var grpcResult = new GrpcResult<ReminderDTO>
    {
      IsSuccess = true,
      Data = reminderDto
    };

    _ = _mockService.Setup(s => s.CreateReminderAsync(It.IsAny<GrpcCreateReminderRequest>()))
      .ReturnsAsync(grpcResult);

    var client = CreateClient();

    // Act
    var result = await client.CreateReminderAsync(createRequest);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
    Assert.Equal("Take a break", result.Value.Details.Value);
  }

  [Fact]
  public async Task CreateReminderAsyncWithServiceErrorReturnsFailAsync()
  {
    // Arrange
    var platformId = new PlatformId("testuser", "123456", Platform.Discord);
    var createRequest = new CoreCreateReminderRequest
    {
      PlatformId = platformId,
      Message = "Test reminder",
      ReminderTime = "in 5 minutes"
    };

    var grpcResult = new GrpcResult<ReminderDTO>
    {
      IsSuccess = false,
      Errors = [new GrpcError("Invalid reminder time", "INVALID_TIME")]
    };

    _ = _mockService.Setup(s => s.CreateReminderAsync(It.IsAny<GrpcCreateReminderRequest>()))
      .ReturnsAsync(grpcResult);

    var client = CreateClient();

    // Act
    var result = await client.CreateReminderAsync(createRequest);

    // Assert
    Assert.True(result.IsFailed);
    Assert.Contains("Invalid reminder time", result.GetErrorMessages());
  }

  [Fact]
  public async Task CreateReminderAsyncPassesCorrectParametersAsync()
  {
    // Arrange
    var platformId = new PlatformId("alice", "999", Platform.Discord);
    var createRequest = new CoreCreateReminderRequest
    {
      PlatformId = platformId,
      Message = "Check the oven",
      ReminderTime = "in 1 hour"
    };

    var reminderDto = new ReminderDTO
    {
      Id = Guid.NewGuid(),
      PersonId = Guid.NewGuid(),
      Details = "Check the oven",
      ReminderTime = DateTime.UtcNow.AddHours(1),
      CreatedOn = DateTime.UtcNow,
      UpdatedOn = DateTime.UtcNow
    };

    var grpcResult = new GrpcResult<ReminderDTO>
    {
      IsSuccess = true,
      Data = reminderDto
    };

    _ = _mockService.Setup(s => s.CreateReminderAsync(It.IsAny<GrpcCreateReminderRequest>()))
      .ReturnsAsync(grpcResult);

    var client = CreateClient();

    // Act
    _ = await client.CreateReminderAsync(createRequest);

    // Assert
    _mockService.Verify(
      s => s.CreateReminderAsync(It.Is<GrpcCreateReminderRequest>(req =>
        req.Username == "alice" &&
        req.PlatformUserId == "999" &&
        req.Platform == Platform.Discord &&
        req.Message == "Check the oven" &&
        req.ReminderTime == "in 1 hour")),
      Times.Once);
  }

  [Fact]
  public async Task CreateReminderAsyncWithDifferentTimesCreatesCorrectlyAsync()
  {
    // Arrange
    var platformId = new PlatformId("testuser", "123456", Platform.Discord);

    var grpcResult = new GrpcResult<ReminderDTO>
    {
      IsSuccess = true,
      Data = new ReminderDTO
      {
        Id = Guid.NewGuid(),
        PersonId = Guid.NewGuid(),
        Details = "Test",
        ReminderTime = DateTime.UtcNow.AddHours(2),
        CreatedOn = DateTime.UtcNow,
        UpdatedOn = DateTime.UtcNow
      }
    };

    _ = _mockService.Setup(s => s.CreateReminderAsync(It.IsAny<GrpcCreateReminderRequest>()))
      .ReturnsAsync(grpcResult);

    var client = CreateClient();

    // Act - Test with different time formats
    var request1 = new CoreCreateReminderRequest
    {
      PlatformId = platformId,
      Message = "Reminder 1",
      ReminderTime = "in 10 minutes"
    };

    var request2 = new CoreCreateReminderRequest
    {
      PlatformId = platformId,
      Message = "Reminder 2",
      ReminderTime = "in 2 hours"
    };

    var result1 = await client.CreateReminderAsync(request1);
    var result2 = await client.CreateReminderAsync(request2);

    // Assert
    Assert.True(result1.IsSuccess);
    Assert.True(result2.IsSuccess);
  }

  [Fact]
  public async Task CreateReminderAsyncWithEmptyMessageReturnsFailAsync()
  {
    // Arrange
    var platformId = new PlatformId("testuser", "123456", Platform.Discord);
    var createRequest = new CoreCreateReminderRequest
    {
      PlatformId = platformId,
      Message = string.Empty,
      ReminderTime = "in 5 minutes"
    };

    var grpcResult = new GrpcResult<ReminderDTO>
    {
      IsSuccess = false,
      Errors = [new GrpcError("Message cannot be empty", "EMPTY_MESSAGE")]
    };

    _ = _mockService.Setup(s => s.CreateReminderAsync(It.IsAny<GrpcCreateReminderRequest>()))
      .ReturnsAsync(grpcResult);

    var client = CreateClient();

    // Act
    var result = await client.CreateReminderAsync(createRequest);

    // Assert
    Assert.True(result.IsFailed);
  }

  [Fact]
  public async Task CreateReminderAsyncMapsPlatformIdCorrectlyAsync()
  {
    // Arrange
    var platformId = new PlatformId("user123", "456789", Platform.Discord);
    var createRequest = new CoreCreateReminderRequest
    {
      PlatformId = platformId,
      Message = "Remember to hydrate",
      ReminderTime = "in 30 minutes"
    };

    var reminderDto = new ReminderDTO
    {
      Id = Guid.NewGuid(),
      PersonId = Guid.NewGuid(),
      Details = "Remember to hydrate",
      ReminderTime = DateTime.UtcNow.AddMinutes(30),
      CreatedOn = DateTime.UtcNow,
      UpdatedOn = DateTime.UtcNow
    };

    var grpcResult = new GrpcResult<ReminderDTO>
    {
      IsSuccess = true,
      Data = reminderDto
    };

    _ = _mockService.Setup(s => s.CreateReminderAsync(It.IsAny<GrpcCreateReminderRequest>()))
      .ReturnsAsync(grpcResult);

    var client = CreateClient();

    // Act
    _ = await client.CreateReminderAsync(createRequest);

    // Assert
    _mockService.Verify(
      s => s.CreateReminderAsync(It.Is<GrpcCreateReminderRequest>(req =>
        req.Username == "user123" &&
        req.PlatformUserId == "456789" &&
        req.Platform == Platform.Discord)),
      Times.Once);
  }

  [Fact]
  public async Task CreateReminderAsyncWithNullReminderDateHandlesGracefullyAsync()
  {
    // Arrange
    var platformId = new PlatformId("testuser", "123456", Platform.Discord);
    var createRequest = new CoreCreateReminderRequest
    {
      PlatformId = platformId,
      Message = "Reminder without specific date",
      ReminderTime = "tomorrow"
    };

    var reminderDto = new ReminderDTO
    {
      Id = Guid.NewGuid(),
      PersonId = Guid.NewGuid(),
      Details = "Reminder without specific date",
      ReminderTime = DateTime.UtcNow.AddDays(1),
      CreatedOn = DateTime.UtcNow,
      UpdatedOn = DateTime.UtcNow
    };

    var grpcResult = new GrpcResult<ReminderDTO>
    {
      IsSuccess = true,
      Data = reminderDto
    };

    _ = _mockService.Setup(s => s.CreateReminderAsync(It.IsAny<GrpcCreateReminderRequest>()))
      .ReturnsAsync(grpcResult);

    var client = CreateClient();

    // Act
    var result = await client.CreateReminderAsync(createRequest);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
  }

  [Fact]
  public async Task CreateReminderAsyncCreatesWithCorrectMessageAsync()
  {
    // Arrange
    var platformId = new PlatformId("testuser", "123456", Platform.Discord);
    const string messageText = "Buy groceries";
    var createRequest = new CoreCreateReminderRequest
    {
      PlatformId = platformId,
      Message = messageText,
      ReminderTime = "in 2 hours"
    };

    var reminderDto = new ReminderDTO
    {
      Id = Guid.NewGuid(),
      PersonId = Guid.NewGuid(),
      Details = messageText,
      ReminderTime = DateTime.UtcNow.AddHours(2),
      CreatedOn = DateTime.UtcNow,
      UpdatedOn = DateTime.UtcNow
    };

    var grpcResult = new GrpcResult<ReminderDTO>
    {
      IsSuccess = true,
      Data = reminderDto
    };

    _ = _mockService.Setup(s => s.CreateReminderAsync(It.IsAny<GrpcCreateReminderRequest>()))
      .ReturnsAsync(grpcResult);

    var client = CreateClient();

    // Act
    var result = await client.CreateReminderAsync(createRequest);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(messageText, result.Value.Details.Value);
  }
}
