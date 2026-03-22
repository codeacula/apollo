using Apollo.Application.Configuration;
using Apollo.GRPC.Contracts;

using MediatR;

namespace Apollo.GRPC.Service;

public sealed class ConfigurationGrpcService(IMediator mediator) : IConfigurationGrpcService
{
  public async Task<GrpcResult<ConfigurationDTO>> GetConfigurationAsync()
  {
    var query = new GetConfigurationQuery();
    var result = await mediator.Send(query);

    if (result.IsFailed)
    {
      return result.Errors.Select(e => new GrpcError(e.Message)).ToArray();
    }

    var config = result.Value;
    return new ConfigurationDTO
    {
      Id = config.Id,
      AiModelId = config.AiModelId,
      AiEndpoint = config.AiEndpoint,
      HasAiApiKey = !string.IsNullOrWhiteSpace(config.AiApiKey),
      HasDiscordToken = !string.IsNullOrWhiteSpace(config.DiscordToken),
      DiscordPublicKey = config.DiscordPublicKey,
      DiscordBotName = config.DiscordBotName,
      SuperAdminDiscordUserId = config.SuperAdminDiscordUserId
    };
  }

  public async Task<GrpcResult<ConfigurationDTO>> UpdateAiConfigurationAsync(UpdateAiConfigurationRequest request)
  {
    var command = new UpdateAiConfigurationCommand(request.ModelId, request.Endpoint, request.ApiKey);
    var result = await mediator.Send(command);

    if (result.IsFailed)
    {
      return result.Errors.Select(e => new GrpcError(e.Message)).ToArray();
    }

    var config = result.Value;
    return new ConfigurationDTO
    {
      Id = config.Id,
      AiModelId = config.AiModelId,
      AiEndpoint = config.AiEndpoint,
      HasAiApiKey = !string.IsNullOrWhiteSpace(config.AiApiKey),
      HasDiscordToken = !string.IsNullOrWhiteSpace(config.DiscordToken),
      DiscordPublicKey = config.DiscordPublicKey,
      DiscordBotName = config.DiscordBotName,
      SuperAdminDiscordUserId = config.SuperAdminDiscordUserId
    };
  }

  public async Task<GrpcResult<ConfigurationDTO>> UpdateDiscordConfigurationAsync(UpdateDiscordConfigurationRequest request)
  {
    var command = new UpdateDiscordConfigurationCommand(request.Token, request.PublicKey, request.BotName);
    var result = await mediator.Send(command);

    if (result.IsFailed)
    {
      return result.Errors.Select(e => new GrpcError(e.Message)).ToArray();
    }

    var config = result.Value;
    return new ConfigurationDTO
    {
      Id = config.Id,
      AiModelId = config.AiModelId,
      AiEndpoint = config.AiEndpoint,
      HasAiApiKey = !string.IsNullOrWhiteSpace(config.AiApiKey),
      HasDiscordToken = !string.IsNullOrWhiteSpace(config.DiscordToken),
      DiscordPublicKey = config.DiscordPublicKey,
      DiscordBotName = config.DiscordBotName,
      SuperAdminDiscordUserId = config.SuperAdminDiscordUserId
    };
  }

  public async Task<GrpcResult<ConfigurationDTO>> UpdateSuperAdminConfigurationAsync(UpdateSuperAdminConfigurationRequest request)
  {
    var command = new UpdateSuperAdminConfigurationCommand(request.DiscordUserId);
    var result = await mediator.Send(command);

    if (result.IsFailed)
    {
      return result.Errors.Select(e => new GrpcError(e.Message)).ToArray();
    }

    var config = result.Value;
    return new ConfigurationDTO
    {
      Id = config.Id,
      AiModelId = config.AiModelId,
      AiEndpoint = config.AiEndpoint,
      HasAiApiKey = !string.IsNullOrWhiteSpace(config.AiApiKey),
      HasDiscordToken = !string.IsNullOrWhiteSpace(config.DiscordToken),
      DiscordPublicKey = config.DiscordPublicKey,
      DiscordBotName = config.DiscordBotName,
      SuperAdminDiscordUserId = config.SuperAdminDiscordUserId
    };
  }

  public async Task<GrpcResult<ConfigurationStatusDTO>> GetConfigurationStatusAsync()
  {
    var query = new GetInitializationStatusQuery();
    var result = await mediator.Send(query);

    if (result.IsFailed)
    {
      return result.Errors.Select(e => new GrpcError(e.Message)).ToArray();
    }

    var status = result.Value;
    return new ConfigurationStatusDTO
    {
      IsInitialized = status.IsInitialized,
      IsAiConfigured = status.IsAiConfigured,
      IsDiscordConfigured = status.IsDiscordConfigured,
      IsSuperAdminConfigured = status.IsSuperAdminConfigured
    };
  }
}
