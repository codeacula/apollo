using System.Runtime.Serialization;

using Apollo.Domain.Common.Enums;

namespace Apollo.GRPC.Contracts;

[DataContract]
public sealed record GetPersonToDosRequest
{
  [DataMember(Order = 1)]
  public required Platform Platform { get; init; }

  [DataMember(Order = 2)]
  public bool IncludeCompleted { get; init; }

  [DataMember(Order = 3)]
  public required string ProviderId { get; init; }
}
