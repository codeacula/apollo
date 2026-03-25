using System.Runtime.Serialization;

namespace Apollo.GRPC.Contracts;

[DataContract]
public sealed record ConfigurationStatusDTO
{
  [DataMember(Order = 1)]
  public required bool IsInitialized { get; init; }

  [DataMember(Order = 2)]
  public required bool IsAiConfigured { get; init; }

  [DataMember(Order = 3)]
  public required bool IsDiscordConfigured { get; init; }

  [DataMember(Order = 4)]
  public required bool IsSuperAdminConfigured { get; init; }
}
