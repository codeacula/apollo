using System.Runtime.Serialization;

namespace Apollo.GRPC.Contracts;

[DataContract]
public sealed record ConfigurationDTO
{
  [DataMember(Order = 1)]
  public required Guid Id { get; init; }

  [DataMember(Order = 2)]
  public string? AiModelId { get; init; }

  [DataMember(Order = 3)]
  public string? AiEndpoint { get; init; }

  [DataMember(Order = 4)]
  public required bool HasAiApiKey { get; init; }

  [DataMember(Order = 5)]
  public required bool HasDiscordToken { get; init; }

  [DataMember(Order = 6)]
  public string? DiscordPublicKey { get; init; }

  [DataMember(Order = 7)]
  public string? DiscordBotName { get; init; }

  [DataMember(Order = 8)]
  public string? SuperAdminDiscordUserId { get; init; }
}
