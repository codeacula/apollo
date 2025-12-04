using Apollo.Core.Infrastructure.Cache;
using Apollo.Core.Infrastructure.Data;
using Apollo.Core.Infrastructure.Services;
using Apollo.Core.Logging;
using Apollo.Domain.Users.Models;
using Apollo.Domain.Users.ValueObjects;

using FluentResults;

using Microsoft.Extensions.Logging;

namespace Apollo.Application.Services;

public sealed class ApolloUserService(
  IApolloUserStore apolloUserStore,
  IUserCache userCache,
  ILogger<ApolloUserService> logger) : IApolloUserService
{
  public async Task<Result<User>> GetOrCreateUserAsync(Username username, CancellationToken cancellationToken = default)
  {
    if (!username.IsValid)
    {
      ValidationLogs.InvalidUsername(logger);
      return Result.Fail<User>("Invalid username");
    }

    var userResult = await apolloUserStore.GetOrCreateUserAsync(username, cancellationToken);

    if (userResult.IsFailed)
    {
      // TODO: Add logging here
      return Result.Fail<User>($"Failed to get or create user {username}");
    }

    return userResult;
  }

  public async Task<Result<bool>> UserHasAccessAsync(Username username, CancellationToken cancellationToken = default)
  {
    if (!username.IsValid)
    {
      ValidationLogs.InvalidUsername(logger);
      return Result.Fail<bool>("Invalid username");
    }

    var cacheResult = await userCache.GetUserAccessAsync(username, cancellationToken);
    if (cacheResult.IsFailed)
    {
      ValidationLogs.CacheCheckFailed(logger, username, string.Join(", ", cacheResult.Errors.Select(e => e.Message)));
      return Result.Fail<bool>($"Cache error for user {username}: fail-closed policy denies access");
    }

    if (cacheResult.Value.HasValue)
    {
      var cachedAccess = cacheResult.Value.Value;
      ValidationLogs.CacheHit(logger, username, cachedAccess);
      return Result.Ok(cachedAccess);
    }

    // Default to returning true because the API will update cache if needed and reject the request
    return Result.Ok(true);
  }
}
