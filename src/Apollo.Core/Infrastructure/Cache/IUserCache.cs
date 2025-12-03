using Apollo.Domain.Users.ValueObjects;

using FluentResults;

namespace Apollo.Core.Infrastructure.Cache;

/// <summary>
/// Provides caching operations for user access validation.
/// </summary>
public interface IUserCache
{
  Task<Result<bool?>> GetUserAccessAsync(Username username, CancellationToken cancellationToken = default);
  Task<Result> SetUserAccessAsync(Username username, bool hasAccess, CancellationToken cancellationToken = default);
  Task<Result> InvalidateUserAccessAsync(Username username, CancellationToken cancellationToken = default);
}
