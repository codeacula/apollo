using System.Runtime.Serialization;

using Apollo.Domain.Common.Enums;

namespace Apollo.Core.ToDos.Requests;

[DataContract]
public sealed record CreateToDoRequest
{
  [DataMember(Order = 1)]
  public required string Username { get; init; }

  [DataMember(Order = 2)]
  public required string Content { get; init; }

  [DataMember(Order = 3)]
  public required Platform Platform { get; init; }
}
