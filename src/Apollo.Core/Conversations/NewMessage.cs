using System.Runtime.Serialization;

namespace Apollo.Core.Conversations;

[DataContract]
public sealed record NewMessage
{
  [DataMember(Order = 1)]
  public required string Username { get; init; }

  [DataMember(Order = 2)]
  public required string Content { get; init; }
}
