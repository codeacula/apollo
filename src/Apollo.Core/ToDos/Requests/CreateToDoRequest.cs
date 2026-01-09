using System.Runtime.Serialization;

using Apollo.Domain.Common.Enums;

namespace Apollo.Core.ToDos.Requests;

[DataContract]
public sealed record CreateToDoRequest
{
  [DataMember(Order = 1)]
  public required Platform Platform { get; init; }

  [DataMember(Order = 2)]
  public required string PlatformId { get; init; }

  [DataMember(Order = 3)]
  public required string Title { get; init; }

  [DataMember(Order = 4)]
  public required string Description { get; init; }
}
