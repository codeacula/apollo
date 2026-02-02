using System.Runtime.Serialization;

using Apollo.Domain.Common.Enums;
using Apollo.Domain.People.ValueObjects;

namespace Apollo.GRPC.Contracts;

[DataContract]
public sealed record CreateToDoRequest : AuthenticatedRequestBase
{
  [DataMember(Order = 1)]
  public required string Title { get; init; }

  [DataMember(Order = 2)]
  public required string Description { get; init; }

  [DataMember(Order = 3)]
  public DateTime? ReminderDate { get; init; }
}
