using System.Runtime.Serialization;

namespace Apollo.GRPC.Contracts;

[DataContract]
public sealed record UpdateDiscordConfigurationRequest
{
  [DataMember(Order = 1)]
  public string? Token { get; init; }

  [DataMember(Order = 2)]
  public string? PublicKey { get; init; }

  [DataMember(Order = 3)]
  public string? BotName { get; init; }
}
