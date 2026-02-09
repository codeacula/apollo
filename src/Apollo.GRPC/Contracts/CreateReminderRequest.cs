using System.Runtime.Serialization;

namespace Apollo.GRPC.Contracts;

[DataContract]
public sealed record CreateReminderRequest : AuthenticatedRequestBase
{
  [DataMember(Order = 1)]
  public required string Message { get; init; }

  [DataMember(Order = 2)]
  public required string ReminderTime { get; init; }
}
