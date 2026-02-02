using System.Runtime.Serialization;

using Apollo.Domain.Common.Enums;

namespace Apollo.GRPC.Contracts;

[DataContract]
public sealed record GetPersonToDosRequest : AuthenticatedRequestBase
{
  [DataMember(Order = 1)]
  public bool IncludeCompleted { get; init; }
}
