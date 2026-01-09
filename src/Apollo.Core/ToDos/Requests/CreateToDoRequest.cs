using System.Runtime.Serialization;

using Apollo.Domain.Common.Enums;

namespace Apollo.Core.ToDos.Requests;

[DataContract]
public sealed record CreateToDoRequest
{
  [DataMember(Order = 1)]
  public required string Username { get; init; }

  [DataMember(Order = 2)]
  public required Platform Platform { get; init; }

  [DataMember(Order = 3)]
  public required string Description { get; init; }

  [DataMember(Order = 4)]
  public DateTime? ReminderDate { get; init; }

  [DataMember(Order = 5)]
  public required string ProviderId { get; init; }
}
