using Apollo.Core.Infrastructure.Cache;
using Apollo.Core.Infrastructure.Data;
using Apollo.Core.Infrastructure.Services;
using Apollo.Core.Logging;
using Apollo.Domain.Users.ValueObjects;

using FluentResults;

using Microsoft.Extensions.Logging;

namespace Apollo.Application.Services;

public sealed class UserValidationService(
  IUserCache userCache,
  IUserDataAccess userDataAccess,
  ILogger<UserValidationService> logger) : IUserValidationService
{
  public async Task<Result<bool>> ValidateUserAccessAsync(Username username, CancellationToken cancellationToken = default)
  {
    if (string.IsNullOrWhiteSpace(username.Value))
    {
      ValidationLogs.InvalidUsername(logger);
      return Result.Fail<bool>("Username cannot be null or empty");
    }

    // Step 1: Check cache
    var cacheResult = await userCache.GetUserAccessAsync(username, cancellationToken);
    if (cacheResult.IsFailed)
    {
      // Fail-closed: cache error denies access
      ValidationLogs.CacheCheckFailed(logger, username.Value, string.Join(", ", cacheResult.Errors.Select(e => e.Message)));
      return Result.Fail<bool>($"Cache error for user {username.Value}: fail-closed policy denies access");
    }

    // Cache hit
    if (cacheResult.Value.HasValue)
    {
      var cachedAccess = cacheResult.Value.Value;
      ValidationLogs.CacheHit(logger, username.Value, cachedAccess);
      return Result.Ok(cachedAccess);
    }

    // Step 2: Cache miss - query data access
    ValidationLogs.CacheMiss(logger, username.Value);
    var dataResult = await userDataAccess.GetUserAccessAsync(username, cancellationToken);
    if (dataResult.IsFailed)
    {
      // Fail-closed: data access error denies access
      ValidationLogs.DataAccessFailed(logger, username.Value, string.Join(", ", dataResult.Errors.Select(e => e.Message)));
      return Result.Fail<bool>($"Data access error for user {username.Value}: fail-closed policy denies access");
    }

    var hasAccess = dataResult.Value;

    // Step 3: Update cache (best effort - don't fail if cache update fails)
    var setCacheResult = await userCache.SetUserAccessAsync(username, hasAccess, cancellationToken);
    if (setCacheResult.IsFailed)
    {
      ValidationLogs.CacheUpdateFailed(logger, username.Value, string.Join(", ", setCacheResult.Errors.Select(e => e.Message)));
      // Continue anyway - we have the answer from data access
    }
    else
    {
      ValidationLogs.CacheUpdated(logger, username.Value, hasAccess);
    }

    ValidationLogs.UserValidated(logger, username.Value, hasAccess);
    return Result.Ok(hasAccess);
  }
}
