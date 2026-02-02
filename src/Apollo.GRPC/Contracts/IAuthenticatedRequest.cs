using Apollo.Domain.Common.Enums;

namespace Apollo.GRPC.Contracts;

public interface IAuthenticatedRequest
{
    Platform Platform { get; }
    string PlatformUserId { get; }
    string Username { get; }
}
