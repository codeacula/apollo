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

using CoreCreateToDoRequest = Apollo.Core.ToDos.Requests.CreateToDoRequest;
using GrpcCreateToDoRequest = Apollo.GRPC.Contracts.CreateToDoRequest;

namespace Apollo.GRPC.Tests;

public sealed class ApolloGrpcClientToDoOperationsTests
{
  private readonly Mock<IApolloGrpcService> _mockService;
  private readonly Mock<ILogger<GrpcClientLoggingInterceptor>> _mockLogger;

  public ApolloGrpcClientToDoOperationsTests()
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
  public async Task CreateToDoAsyncWithValidRequestReturnsToDoAsync()
  {
    // Arrange
    var platformId = new PlatformId("testuser", "123456", Platform.Discord);
    var createRequest = new CoreCreateToDoRequest
    {
      PlatformId = platformId,
      Title = "Buy groceries",
      Description = "Milk, bread, eggs",
      ReminderDate = null
    };

    var toDoDto = new ToDoDTO
    {
      Id = Guid.NewGuid(),
      PersonId = Guid.NewGuid(),
      Description = "Buy groceries",
      Priority = Level.Green,
      Energy = Level.Green,
      Interest = Level.Green,
      CreatedOn = DateTime.UtcNow,
      UpdatedOn = DateTime.UtcNow,
      ReminderDate = null
    };

    var grpcResult = new GrpcResult<ToDoDTO>
    {
      IsSuccess = true,
      Data = toDoDto
    };

    _ = _mockService.Setup(s => s.CreateToDoAsync(It.IsAny<GrpcCreateToDoRequest>()))
      .ReturnsAsync(grpcResult);

    var client = CreateClient();

    // Act
    var result = await client.CreateToDoAsync(createRequest);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
    Assert.Equal("Buy groceries", result.Value.Description.Value);
  }

  [Fact]
  public async Task CreateToDoAsyncWithServiceErrorReturnsFailAsync()
  {
    // Arrange
    var platformId = new PlatformId("testuser", "123456", Platform.Discord);
    var createRequest = new CoreCreateToDoRequest
    {
      PlatformId = platformId,
      Title = "Test",
      Description = "Test",
      ReminderDate = null
    };

    var grpcResult = new GrpcResult<ToDoDTO>
    {
      IsSuccess = false,
      Errors = [new GrpcError("Service error", "ERROR")]
    };

    _ = _mockService.Setup(s => s.CreateToDoAsync(It.IsAny<GrpcCreateToDoRequest>()))
      .ReturnsAsync(grpcResult);

    var client = CreateClient();

    // Act
    var result = await client.CreateToDoAsync(createRequest);

    // Assert
    Assert.True(result.IsFailed);
  }

  [Fact]
  public async Task GetToDosAsyncWithEmptyListReturnsEmptyEnumerableAsync()
  {
    // Arrange
    var platformId = new PlatformId("testuser", "123456", Platform.Discord);

    var grpcResult = new GrpcResult<ToDoDTO[]>
    {
      IsSuccess = true,
      Data = []
    };

    _ = _mockService.Setup(s => s.GetPersonToDosAsync(It.IsAny<GetPersonToDosRequest>()))
      .ReturnsAsync(grpcResult);

    var client = CreateClient();

    // Act
    var result = await client.GetToDosAsync(platformId, false);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Empty(result.Value);
  }

  [Fact]
  public async Task GetToDosAsyncWithMultipleToDosReturnsAllAsync()
  {
    // Arrange
    var platformId = new PlatformId("testuser", "123456", Platform.Discord);
    var todos = new[]
    {
      new ToDoDTO
      {
        Id = Guid.NewGuid(),
        PersonId = Guid.NewGuid(),
        Description = "Task 1",
        Priority = Level.Green,
        Energy = Level.Green,
        Interest = Level.Green,
        CreatedOn = DateTime.UtcNow,
        UpdatedOn = DateTime.UtcNow,
        ReminderDate = null
      },
      new ToDoDTO
      {
        Id = Guid.NewGuid(),
        PersonId = Guid.NewGuid(),
        Description = "Task 2",
        Priority = Level.Yellow,
        Energy = Level.Yellow,
        Interest = Level.Yellow,
        CreatedOn = DateTime.UtcNow,
        UpdatedOn = DateTime.UtcNow,
        ReminderDate = null
      }
    };

    var grpcResult = new GrpcResult<ToDoDTO[]>
    {
      IsSuccess = true,
      Data = todos
    };

    _ = _mockService.Setup(s => s.GetPersonToDosAsync(It.IsAny<GetPersonToDosRequest>()))
      .ReturnsAsync(grpcResult);

    var client = CreateClient();

    // Act
    var result = await client.GetToDosAsync(platformId, false);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(2, result.Value.Count());
  }

  [Fact]
  public async Task SetToDoEnergyAsyncWithValidLevelUpdatesEnergyAsync()
  {
    // Arrange
    var platformId = new PlatformId("testuser", "123456", Platform.Discord);
    var toDoId = Guid.NewGuid();

    var grpcResult = new GrpcResult<string>
    {
      IsSuccess = true,
      Data = "Energy updated"
    };

    _ = _mockService.Setup(s => s.SetToDoEnergyAsync(It.IsAny<SetToDoEnergyRequest>()))
      .ReturnsAsync(grpcResult);

    var client = CreateClient();

    // Act
    var result = await client.SetToDoEnergyAsync(platformId, toDoId, Level.Blue);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal("Energy updated", result.Value);
  }

  [Fact]
  public async Task SetToDoEnergyAsyncWithDifferentLevelsUpdatesCorrectlyAsync()
  {
    // Arrange
    var platformId = new PlatformId("testuser", "123456", Platform.Discord);
    var toDoId = Guid.NewGuid();

    var grpcResult = new GrpcResult<string>
    {
      IsSuccess = true,
      Data = "Energy updated"
    };

    _ = _mockService.Setup(s => s.SetToDoEnergyAsync(It.IsAny<SetToDoEnergyRequest>()))
      .ReturnsAsync(grpcResult);

    var client = CreateClient();

    // Act - Test with different levels
    var resultBlue = await client.SetToDoEnergyAsync(platformId, toDoId, Level.Blue);
    var resultGreen = await client.SetToDoEnergyAsync(platformId, toDoId, Level.Green);
    var resultYellow = await client.SetToDoEnergyAsync(platformId, toDoId, Level.Yellow);
    var resultRed = await client.SetToDoEnergyAsync(platformId, toDoId, Level.Red);

    // Assert
    Assert.True(resultBlue.IsSuccess);
    Assert.True(resultGreen.IsSuccess);
    Assert.True(resultYellow.IsSuccess);
    Assert.True(resultRed.IsSuccess);
  }

  [Fact]
  public async Task SetToDoEnergyAsyncWithErrorReturnsFailAsync()
  {
    // Arrange
    var platformId = new PlatformId("testuser", "123456", Platform.Discord);
    var toDoId = Guid.NewGuid();

    var grpcResult = new GrpcResult<string>
    {
      IsSuccess = false,
      Errors = [new GrpcError("Not found", "NOT_FOUND")]
    };

    _ = _mockService.Setup(s => s.SetToDoEnergyAsync(It.IsAny<SetToDoEnergyRequest>()))
      .ReturnsAsync(grpcResult);

    var client = CreateClient();

    // Act
    var result = await client.SetToDoEnergyAsync(platformId, toDoId, Level.Green);

    // Assert
    Assert.True(result.IsFailed);
    Assert.Contains("Not found", result.GetErrorMessages());
  }

  [Fact]
  public async Task GetToDosAsyncPassesIncludeCompletedParameterAsync()
  {
    // Arrange
    var platformId = new PlatformId("testuser", "123456", Platform.Discord);

    var grpcResult = new GrpcResult<ToDoDTO[]>
    {
      IsSuccess = true,
      Data = []
    };

    _ = _mockService.Setup(s => s.GetPersonToDosAsync(It.IsAny<GetPersonToDosRequest>()))
      .ReturnsAsync(grpcResult);

    var client = CreateClient();

    // Act
    _ = await client.GetToDosAsync(platformId, true);

    // Assert
    _mockService.Verify(
      s => s.GetPersonToDosAsync(It.Is<GetPersonToDosRequest>(req =>
        req.IncludeCompleted)),
      Times.Once);
  }

  [Fact]
  public async Task CreateToDoAsyncMapsPlatformIdCorrectlyAsync()
  {
    // Arrange
    var platformId = new PlatformId("alice", "999", Platform.Discord);
    var createRequest = new CoreCreateToDoRequest
    {
      PlatformId = platformId,
      Title = "Test",
      Description = "Test",
      ReminderDate = null
    };

    var toDoDto = new ToDoDTO
    {
      Id = Guid.NewGuid(),
      PersonId = Guid.NewGuid(),
      Description = "Test",
      Priority = Level.Green,
      Energy = Level.Green,
      Interest = Level.Green,
      CreatedOn = DateTime.UtcNow,
      UpdatedOn = DateTime.UtcNow,
      ReminderDate = null
    };

    var grpcResult = new GrpcResult<ToDoDTO>
    {
      IsSuccess = true,
      Data = toDoDto
    };

    _ = _mockService.Setup(s => s.CreateToDoAsync(It.IsAny<GrpcCreateToDoRequest>()))
      .ReturnsAsync(grpcResult);

    var client = CreateClient();

    // Act
    _ = await client.CreateToDoAsync(createRequest);

    // Assert
    _mockService.Verify(
      s => s.CreateToDoAsync(It.Is<GrpcCreateToDoRequest>(req =>
        req.Username == "alice" &&
        req.PlatformUserId == "999" &&
        req.Platform == Platform.Discord)),
      Times.Once);
  }

  [Fact]
  public async Task GetToDosAsyncErrorReturnsFailAsync()
  {
    // Arrange
    var platformId = new PlatformId("testuser", "123456", Platform.Discord);

    var grpcResult = new GrpcResult<ToDoDTO[]>
    {
      IsSuccess = false,
      Errors = [new GrpcError("Fetch failed", "ERROR")]
    };

    _ = _mockService.Setup(s => s.GetPersonToDosAsync(It.IsAny<GetPersonToDosRequest>()))
      .ReturnsAsync(grpcResult);

    var client = CreateClient();

    // Act
    var result = await client.GetToDosAsync(platformId);

    // Assert
    Assert.True(result.IsFailed);
  }

  [Fact]
  public async Task SetToDoEnergyAsyncPassesCorrectParametersAsync()
  {
    // Arrange
    var platformId = new PlatformId("testuser", "123456", Platform.Discord);
    var toDoId = Guid.NewGuid();

    var grpcResult = new GrpcResult<string>
    {
      IsSuccess = true,
      Data = "Updated"
    };

    _ = _mockService.Setup(s => s.SetToDoEnergyAsync(It.IsAny<SetToDoEnergyRequest>()))
      .ReturnsAsync(grpcResult);

    var client = CreateClient();

    // Act
    _ = await client.SetToDoEnergyAsync(platformId, toDoId, Level.Red);

    // Assert
    _mockService.Verify(
      s => s.SetToDoEnergyAsync(It.Is<SetToDoEnergyRequest>(req =>
        req.PlatformUserId == "123456" &&
        req.Username == "testuser" &&
        req.Platform == Platform.Discord &&
        req.ToDoId == toDoId &&
        req.Energy == Level.Red)),
      Times.Once);
  }
}
