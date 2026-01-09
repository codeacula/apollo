using System.Runtime.Serialization;

using Apollo.Domain.Common.Enums;

namespace Apollo.GRPC.Contracts;

[DataContract]
public sealed record GetPersonToDosRequest
{
  [DataMember(Order = 1)]
  public required string ProviderId { get; init; }

  [DataMember(Order = 2)]
  public required Platform Platform { get; init; }

  [DataMember(Order = 3)]
  public bool IncludeCompleted { get; init; }
}
