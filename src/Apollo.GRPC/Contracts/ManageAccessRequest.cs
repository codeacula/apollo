using System.Runtime.Serialization;

using Apollo.Domain.Common.Enums;
using Apollo.Domain.People.ValueObjects;

namespace Apollo.GRPC.Contracts;

[DataContract]
public sealed record ManageAccessRequest : IAuthenticatedRequest
{
  Platform IAuthenticatedRequest.Platform => AdminPlatform;
  string IAuthenticatedRequest.PlatformUserId => AdminPlatformUserId;
  string IAuthenticatedRequest.Username => AdminUsername;
  /// <summary>
  /// The platform of the admin making the request.
  /// </summary>
  [DataMember(Order = 1)]
  public required Platform AdminPlatform { get; init; }

  /// <summary>
  /// The platform user ID of the admin making the request.
  /// </summary>
  [DataMember(Order = 2)]
  public required string AdminPlatformUserId { get; init; }

  /// <summary>
  /// The username of the admin making the request.
  /// </summary>
  [DataMember(Order = 3)]
  public required string AdminUsername { get; init; }

  /// <summary>
  /// The platform of the target user whose access is being managed.
  /// </summary>
  [DataMember(Order = 4)]
  public required Platform TargetPlatform { get; init; }

  /// <summary>
  /// The platform user ID of the target user whose access is being managed.
  /// </summary>
  [DataMember(Order = 5)]
  public required string TargetPlatformUserId { get; init; }

  /// <summary>
  /// The username of the target user whose access is being managed.
  /// </summary>
  [DataMember(Order = 6)]
  public required string TargetUsername { get; init; }

  public PlatformId ToAdminPlatformId() => new(AdminUsername, AdminPlatformUserId, AdminPlatform);
  public PlatformId ToTargetPlatformId() => new(TargetUsername, TargetPlatformUserId, TargetPlatform);
}
