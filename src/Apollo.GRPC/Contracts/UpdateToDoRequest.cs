using System.Runtime.Serialization;

using Apollo.Domain.Common.Enums;

namespace Apollo.GRPC.Contracts;

[DataContract]
public sealed record UpdateToDoRequest : IAuthenticatedRequest
{
  [DataMember(Order = 1)]
  public required string Username { get; init; }

  [DataMember(Order = 2)]
  public required string PlatformUserId { get; init; }

  [DataMember(Order = 3)]
  public required Platform Platform { get; init; }

  [DataMember(Order = 4)]
  public required Guid ToDoId { get; init; }

  [DataMember(Order = 5)]
  public required string Description { get; init; }
}
