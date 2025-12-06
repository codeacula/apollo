using System.Runtime.Serialization;

using Apollo.Domain.Common.Enums;

namespace Apollo.Core.Conversations;

[DataContract]
public sealed record NewMessage
{
  [DataMember(Order = 1)]
  public required string Username { get; init; }

  [DataMember(Order = 2)]
  public required string Content { get; init; }

  [DataMember(Order = 3)]
  public required Platform Platform { get; init; }
}
