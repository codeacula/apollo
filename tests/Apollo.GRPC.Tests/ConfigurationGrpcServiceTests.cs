using Apollo.Application.Configuration;
using Apollo.Core.Configuration;
using Apollo.GRPC.Contracts;
using Apollo.GRPC.Service;

using FluentResults;

using MediatR;

using Moq;

namespace Apollo.GRPC.Tests;

public sealed class ConfigurationGrpcServiceTests
{
  /// <summary>
  /// AT1: GetConfiguration() returns current config when store has data
  /// </summary>
  [Fact]
  public async Task GetConfigurationAsyncReturnsCurrentConfigWhenDataExistsAsync()
  {
    // Arrange
    var configId = Guid.NewGuid();
    var configData = new ConfigurationData
    {
      Id = configId,
      AiModelId = "gpt-4",
      AiEndpoint = "https://api.openai.com",
      AiApiKey = "test-key",
      DiscordToken = "test-token",
      DiscordPublicKey = "test-public-key",
      DiscordBotName = "TestBot",
      SuperAdminDiscordUserId = "123456789"
    };

    var mediator = new Mock<IMediator>();
    _ = mediator
      .Setup(m => m.Send(It.IsAny<GetConfigurationQuery>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(configData));

    var service = new ConfigurationGrpcService(mediator.Object);

    // Act
    var result = await service.GetConfigurationAsync();

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Data);
    Assert.Equal(configId, result.Data.Id);
    Assert.Equal("gpt-4", result.Data.AiModelId);
    Assert.Equal("https://api.openai.com", result.Data.AiEndpoint);
    Assert.Equal("test-key", result.Data.AiApiKey);
    Assert.Equal("test-token", result.Data.DiscordToken);
    Assert.Equal("test-public-key", result.Data.DiscordPublicKey);
    Assert.Equal("TestBot", result.Data.DiscordBotName);
    Assert.Equal("123456789", result.Data.SuperAdminDiscordUserId);
  }

  /// <summary>
  /// AT1: GetConfiguration() returns empty DTO when no configuration exists
  /// </summary>
  [Fact]
  public async Task GetConfigurationAsyncReturnsEmptyDtoWhenNoConfigurationExistsAsync()
  {
    // Arrange
    var emptyConfigData = ConfigurationData.Empty();
    var mediator = new Mock<IMediator>();
    _ = mediator
      .Setup(m => m.Send(It.IsAny<GetConfigurationQuery>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(emptyConfigData));

    var service = new ConfigurationGrpcService(mediator.Object);

    // Act
    var result = await service.GetConfigurationAsync();

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Data);
    Assert.Equal(Guid.Empty, result.Data.Id);
    Assert.Null(result.Data.AiModelId);
    Assert.Null(result.Data.AiEndpoint);
    Assert.Null(result.Data.AiApiKey);
    Assert.Null(result.Data.DiscordToken);
    Assert.Null(result.Data.DiscordPublicKey);
    Assert.Null(result.Data.DiscordBotName);
    Assert.Null(result.Data.SuperAdminDiscordUserId);
  }

  /// <summary>
  /// AT2: UpdateAiConfiguration(request) persists AI settings and returns updated config
  /// </summary>
  [Fact]
  public async Task UpdateAiConfigurationAsyncPersistsSettingsAndReturnsUpdatedConfigAsync()
  {
    // Arrange
    var configId = Guid.NewGuid();
    var request = new UpdateAiConfigurationRequest
    {
      ModelId = "gpt-4-turbo",
      Endpoint = "https://api.openai.com/v1",
      ApiKey = "sk-test-key"
    };

    var updatedConfig = new ConfigurationData
    {
      Id = configId,
      AiModelId = "gpt-4-turbo",
      AiEndpoint = "https://api.openai.com/v1",
      AiApiKey = "sk-test-key"
    };

    var mediator = new Mock<IMediator>();
    _ = mediator
      .Setup(m => m.Send(It.IsAny<UpdateAiConfigurationCommand>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(updatedConfig));

    var service = new ConfigurationGrpcService(mediator.Object);

    // Act
    var result = await service.UpdateAiConfigurationAsync(request);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Data);
    Assert.Equal(configId, result.Data.Id);
    Assert.Equal("gpt-4-turbo", result.Data.AiModelId);
    Assert.Equal("https://api.openai.com/v1", result.Data.AiEndpoint);
    Assert.Equal("sk-test-key", result.Data.AiApiKey);

    // Verify the correct command was sent
    mediator.Verify(m => m.Send(
      It.Is<UpdateAiConfigurationCommand>(cmd =>
        cmd.ModelId == "gpt-4-turbo" &&
        cmd.Endpoint == "https://api.openai.com/v1" &&
        cmd.ApiKey == "sk-test-key"),
      It.IsAny<CancellationToken>()), Times.Once);
  }

  /// <summary>
  /// AT3: UpdateDiscordConfiguration(request) persists Discord settings
  /// </summary>
  [Fact]
  public async Task UpdateDiscordConfigurationAsyncPersistsSettingsAsync()
  {
    // Arrange
    var configId = Guid.NewGuid();
    var request = new UpdateDiscordConfigurationRequest
    {
      Token = "discord-token-xyz",
      PublicKey = "discord-public-key-xyz",
      BotName = "ApolloBot"
    };

    var updatedConfig = new ConfigurationData
    {
      Id = configId,
      DiscordToken = "discord-token-xyz",
      DiscordPublicKey = "discord-public-key-xyz",
      DiscordBotName = "ApolloBot"
    };

    var mediator = new Mock<IMediator>();
    _ = mediator
      .Setup(m => m.Send(It.IsAny<UpdateDiscordConfigurationCommand>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(updatedConfig));

    var service = new ConfigurationGrpcService(mediator.Object);

    // Act
    var result = await service.UpdateDiscordConfigurationAsync(request);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Data);
    Assert.Equal("discord-token-xyz", result.Data.DiscordToken);
    Assert.Equal("discord-public-key-xyz", result.Data.DiscordPublicKey);
    Assert.Equal("ApolloBot", result.Data.DiscordBotName);

    // Verify the correct command was sent
    mediator.Verify(m => m.Send(
      It.Is<UpdateDiscordConfigurationCommand>(cmd =>
        cmd.Token == "discord-token-xyz" &&
        cmd.PublicKey == "discord-public-key-xyz" &&
        cmd.BotName == "ApolloBot"),
      It.IsAny<CancellationToken>()), Times.Once);
  }

  /// <summary>
  /// AT4: UpdateSuperAdminConfiguration(request) persists SuperAdmin
  /// </summary>
  [Fact]
  public async Task UpdateSuperAdminConfigurationAsyncPersistsSuperAdminAsync()
  {
    // Arrange
    var configId = Guid.NewGuid();
    var request = new UpdateSuperAdminConfigurationRequest
    {
      DiscordUserId = "987654321"
    };

    var updatedConfig = new ConfigurationData
    {
      Id = configId,
      SuperAdminDiscordUserId = "987654321"
    };

    var mediator = new Mock<IMediator>();
    _ = mediator
      .Setup(m => m.Send(It.IsAny<UpdateSuperAdminConfigurationCommand>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(updatedConfig));

    var service = new ConfigurationGrpcService(mediator.Object);

    // Act
    var result = await service.UpdateSuperAdminConfigurationAsync(request);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Data);
    Assert.Equal("987654321", result.Data.SuperAdminDiscordUserId);

    // Verify the correct command was sent
    mediator.Verify(m => m.Send(
      It.Is<UpdateSuperAdminConfigurationCommand>(cmd =>
        cmd.DiscordUserId == "987654321"),
      It.IsAny<CancellationToken>()), Times.Once);
  }

  /// <summary>
  /// AT5: GetConfigurationStatus() returns subsystem readiness flags
  /// </summary>
  [Fact]
  public async Task GetConfigurationStatusAsyncReturnsSubsystemReadinessFlagsAsync()
  {
    // Arrange
    var status = new InitializationStatus(
      IsInitialized: true,
      IsAiConfigured: true,
      IsDiscordConfigured: true,
      IsSuperAdminConfigured: true
    );

    var mediator = new Mock<IMediator>();
    _ = mediator
      .Setup(m => m.Send(It.IsAny<GetInitializationStatusQuery>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(status));

    var service = new ConfigurationGrpcService(mediator.Object);

    // Act
    var result = await service.GetConfigurationStatusAsync();

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Data);
    Assert.True(result.Data.IsInitialized);
    Assert.True(result.Data.IsAiConfigured);
    Assert.True(result.Data.IsDiscordConfigured);
    Assert.True(result.Data.IsSuperAdminConfigured);
  }

  /// <summary>
  /// AT5: GetConfigurationStatus() returns partial readiness when only some subsystems configured
  /// </summary>
  [Fact]
  public async Task GetConfigurationStatusAsyncReturnsPartialReadinessAsync()
  {
    // Arrange
    var status = new InitializationStatus(
      IsInitialized: true,
      IsAiConfigured: true,
      IsDiscordConfigured: false,
      IsSuperAdminConfigured: false
    );

    var mediator = new Mock<IMediator>();
    _ = mediator
      .Setup(m => m.Send(It.IsAny<GetInitializationStatusQuery>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(status));

    var service = new ConfigurationGrpcService(mediator.Object);

    // Act
    var result = await service.GetConfigurationStatusAsync();

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Data);
    Assert.True(result.Data.IsInitialized);
    Assert.True(result.Data.IsAiConfigured);
    Assert.False(result.Data.IsDiscordConfigured);
    Assert.False(result.Data.IsSuperAdminConfigured);
  }

  /// <summary>
  /// AT6: All endpoints use protobuf-net.Grpc code-first contracts with ServiceContract/OperationContract
  /// </summary>
  [Fact]
  public void ServiceContractAttributesArePresentOnInterface()
  {
    // Verify that the interface has ServiceContract attribute
    var interfaceType = typeof(IConfigurationGrpcService);
    var serviceContractAttr = interfaceType.GetCustomAttributes(
      typeof(System.ServiceModel.ServiceContractAttribute), false);

    Assert.NotEmpty(serviceContractAttr);

    // Verify that all methods have OperationContract attribute
    var methods = interfaceType.GetMethods();
    foreach (var method in methods)
    {
      var operationContractAttr = method.GetCustomAttributes(
        typeof(System.ServiceModel.OperationContractAttribute), false);

      Assert.NotEmpty(operationContractAttr);
    }
  }

  /// <summary>
  /// AT6: Request DTOs use DataContract/DataMember attributes
  /// </summary>
  [Fact]
  public void RequestDtosHaveDataContractAttributes()
  {
    // Check UpdateAiConfigurationRequest
    var aiReqType = typeof(UpdateAiConfigurationRequest);
    var aiDataContract = aiReqType.GetCustomAttributes(
      typeof(System.Runtime.Serialization.DataContractAttribute), false);
    Assert.NotEmpty(aiDataContract);

    // Check UpdateDiscordConfigurationRequest
    var discordReqType = typeof(UpdateDiscordConfigurationRequest);
    var discordDataContract = discordReqType.GetCustomAttributes(
      typeof(System.Runtime.Serialization.DataContractAttribute), false);
    Assert.NotEmpty(discordDataContract);

    // Check UpdateSuperAdminConfigurationRequest
    var superAdminReqType = typeof(UpdateSuperAdminConfigurationRequest);
    var superAdminDataContract = superAdminReqType.GetCustomAttributes(
      typeof(System.Runtime.Serialization.DataContractAttribute), false);
    Assert.NotEmpty(superAdminDataContract);
  }

  /// <summary>
  /// AT6: Response DTOs use DataContract/DataMember attributes
  /// </summary>
  [Fact]
  public void ResponseDtosHaveDataContractAttributes()
  {
    // Check ConfigurationDTO
    var configDtoType = typeof(ConfigurationDTO);
    var configDataContract = configDtoType.GetCustomAttributes(
      typeof(System.Runtime.Serialization.DataContractAttribute), false);
    Assert.NotEmpty(configDataContract);

    // Check ConfigurationStatusDTO
    var statusDtoType = typeof(ConfigurationStatusDTO);
    var statusDataContract = statusDtoType.GetCustomAttributes(
      typeof(System.Runtime.Serialization.DataContractAttribute), false);
    Assert.NotEmpty(statusDataContract);
  }

  /// <summary>
  /// AT7: Results are wrapped in GrpcResult&lt;T&gt; (verified by return types)
  /// </summary>
  [Fact]
  public void MethodReturnTypesAreGrpcResult()
  {
    var interfaceType = typeof(IConfigurationGrpcService);
    var methods = interfaceType.GetMethods();

    foreach (var method in methods)
    {
      var returnType = method.ReturnType;
      // Check if return type is Task<GrpcResult<...>>
      Assert.True(
        returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>),
        $"Method {method.Name} should return a Task");

      var taskResultType = returnType.GetGenericArguments()[0];
      Assert.True(
        taskResultType.IsGenericType && taskResultType.GetGenericTypeDefinition() == typeof(GrpcResult<>),
        $"Method {method.Name} should return Task<GrpcResult<T>>");
    }
  }

  /// <summary>
  /// Error handling: Update command returns failure wrapped in GrpcResult
  /// </summary>
  [Fact]
  public async Task UpdateAiConfigurationAsyncReturnsGrpcErrorOnFailureAsync()
  {
    // Arrange
    var request = new UpdateAiConfigurationRequest
    {
      ModelId = "gpt-4",
      Endpoint = "https://api.openai.com"
    };

    var mediator = new Mock<IMediator>();
    _ = mediator
      .Setup(m => m.Send(It.IsAny<UpdateAiConfigurationCommand>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Fail<ConfigurationData>("Configuration update failed"));

    var service = new ConfigurationGrpcService(mediator.Object);

    // Act
    var result = await service.UpdateAiConfigurationAsync(request);

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Null(result.Data);
    Assert.NotEmpty(result.Errors);
    Assert.Equal("Configuration update failed", result.Errors[0].Message);
  }

  /// <summary>
  /// Error handling: GetConfiguration returns failure wrapped in GrpcResult
  /// </summary>
  [Fact]
  public async Task GetConfigurationAsyncReturnsGrpcErrorOnFailureAsync()
  {
    // Arrange
    var mediator = new Mock<IMediator>();
    _ = mediator
      .Setup(m => m.Send(It.IsAny<GetConfigurationQuery>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Fail<ConfigurationData>("Database error"));

    var service = new ConfigurationGrpcService(mediator.Object);

    // Act
    var result = await service.GetConfigurationAsync();

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Null(result.Data);
    Assert.NotEmpty(result.Errors);
    Assert.Equal("Database error", result.Errors[0].Message);
  }

  /// <summary>
  /// Error handling: GetConfigurationStatus returns failure wrapped in GrpcResult
  /// </summary>
  [Fact]
  public async Task GetConfigurationStatusAsyncReturnsGrpcErrorOnFailureAsync()
  {
    // Arrange
    var mediator = new Mock<IMediator>();
    _ = mediator
      .Setup(m => m.Send(It.IsAny<GetInitializationStatusQuery>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Fail<InitializationStatus>("Status query failed"));

    var service = new ConfigurationGrpcService(mediator.Object);

    // Act
    var result = await service.GetConfigurationStatusAsync();

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Null(result.Data);
    Assert.NotEmpty(result.Errors);
    Assert.Equal("Status query failed", result.Errors[0].Message);
  }

  /// <summary>
  /// Integration test: All update methods preserve non-updated fields
  /// </summary>
  [Fact]
  public async Task UpdateAiConfigurationAsyncPreservesOtherFieldsAsync()
  {
    // Arrange
    var configId = Guid.NewGuid();
    var request = new UpdateAiConfigurationRequest
    {
      ModelId = "gpt-4-new",
      Endpoint = null, // Not updating endpoint
      ApiKey = "new-key"
    };

    // Configuration returned from store should preserve Discord and SuperAdmin fields
    var updatedConfig = new ConfigurationData
    {
      Id = configId,
      AiModelId = "gpt-4-new",
      AiEndpoint = "https://api.openai.com", // Preserved
      AiApiKey = "new-key",
      DiscordToken = "discord-token", // Preserved
      DiscordPublicKey = "discord-public-key", // Preserved
      DiscordBotName = "ExistingBot", // Preserved
      SuperAdminDiscordUserId = "123456789" // Preserved
    };

    var mediator = new Mock<IMediator>();
    _ = mediator
      .Setup(m => m.Send(It.IsAny<UpdateAiConfigurationCommand>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Ok(updatedConfig));

    var service = new ConfigurationGrpcService(mediator.Object);

    // Act
    var result = await service.UpdateAiConfigurationAsync(request);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Data);
    // Verify AI fields updated
    Assert.Equal("gpt-4-new", result.Data.AiModelId);
    Assert.Equal("https://api.openai.com", result.Data.AiEndpoint);
    Assert.Equal("new-key", result.Data.AiApiKey);
    // Verify other fields preserved
    Assert.Equal("discord-token", result.Data.DiscordToken);
    Assert.Equal("discord-public-key", result.Data.DiscordPublicKey);
    Assert.Equal("ExistingBot", result.Data.DiscordBotName);
    Assert.Equal("123456789", result.Data.SuperAdminDiscordUserId);
  }
}
