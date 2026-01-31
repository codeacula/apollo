using System.Runtime.Serialization;

using Apollo.Domain.People.ValueObjects;

namespace Apollo.Core.Reminders.Requests;

[DataContract]
public sealed record CreateReminderRequest
{
  [DataMember(Order = 1)]
  public required PlatformId PlatformId { get; init; }

  [DataMember(Order = 2)]
  public required string Message { get; init; }

  [DataMember(Order = 3)]
  public required string ReminderTime { get; init; }
}
