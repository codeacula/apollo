using System.Runtime.Serialization;

using Apollo.Domain.People.ValueObjects;

namespace Apollo.Core.Conversations;

[DataContract]
public sealed record NewMessageRequest
{
  [DataMember(Order = 1)]
  public required PlatformId PlatformId { get; init; }

  [DataMember(Order = 2)]
  public required string Content { get; init; }
}
