using System.Runtime.Serialization;

namespace Apollo.GRPC.Contracts;

[DataContract]
public sealed record UpdateAiConfigurationRequest
{
  [DataMember(Order = 1)]
  public string? ModelId { get; init; }

  [DataMember(Order = 2)]
  public string? Endpoint { get; init; }

  [DataMember(Order = 3)]
  public string? ApiKey { get; init; }
}
