using System.Runtime.Serialization;

namespace Apollo.GRPC.Contracts;

[DataContract]
public sealed record ConfigurationDTO
{
  [DataMember(Order = 1)]
  public required string Key { get; init; }

  [DataMember(Order = 2)]
  public required string Value { get; init; }
}

[DataContract]
public sealed record SetConfigurationRequest
{
  [DataMember(Order = 1)]
  public required string Key { get; init; }

  [DataMember(Order = 2)]
  public required string Value { get; init; }
}

[DataContract]
public sealed record DeleteConfigurationRequest
{
  [DataMember(Order = 1)]
  public required string Key { get; init; }
}

[DataContract]
public sealed record GetConfigurationRequest
{
  [DataMember(Order = 1)]
  public required string Key { get; init; }
}

[DataContract]
public sealed record GetAllConfigurationsRequest;
