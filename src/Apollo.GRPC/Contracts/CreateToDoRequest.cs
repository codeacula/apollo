using System.Runtime.Serialization;

using Apollo.Domain.Common.Enums;
using Apollo.Domain.People.ValueObjects;

namespace Apollo.GRPC.Contracts;

[DataContract]
public sealed record CreateToDoRequest
{
  [DataMember(Order = 1)]
  public required Platform Platform { get; init; }

  [DataMember(Order = 2)]
  public required string PlatformUserId { get; init; }

  [DataMember(Order = 3)]
  public required string Username { get; init; }

  [DataMember(Order = 4)]
  public required string Title { get; init; }

  [DataMember(Order = 5)]
  public required string Description { get; init; }

  [DataMember(Order = 6)]
  public DateTime? ReminderDate { get; init; }

  public PlatformId ToPlatformId() => new(Username, PlatformUserId, Platform);
}
