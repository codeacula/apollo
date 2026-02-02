using System.Runtime.Serialization;

using Apollo.Domain.Common.Enums;
using Apollo.Domain.People.ValueObjects;

namespace Apollo.GRPC.Contracts;

[DataContract]
public sealed record DeleteToDoRequest : AuthenticatedRequestBase
{
  [DataMember(Order = 1)]
  public required Guid ToDoId { get; init; }
}
