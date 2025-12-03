using Apollo.Core.Infrastructure.Data;
using Apollo.Core.Logging;
using Apollo.Domain.Users.ValueObjects;

using FluentResults;

using Microsoft.Extensions.Logging;

namespace Apollo.Database.Repository;

/// <summary>
/// Mock implementation of user data access that returns hardcoded values without database queries.
/// This is a temporary implementation for testing the caching layer.
/// </summary>
/// <param name="logger">Logger instance.</param>
public sealed class MockUserDataAccess(ILogger<MockUserDataAccess> logger) : IUserDataAccess
{
  private readonly ILogger<MockUserDataAccess> _logger = logger;

  /// <summary>
  /// Hardcoded list of users with access.
  /// </summary>
  private static readonly HashSet<string> UsersWithAccess =
  [
    "codeacula"
  ];

  public Task<Result<bool>> GetUserAccessAsync(Username username, CancellationToken cancellationToken = default)
  {
    if (string.IsNullOrWhiteSpace(username.Value))
    {
      DataAccessLogs.UserNotFound(_logger, username.Value);
      return Task.FromResult(Result.Fail<bool>("Username cannot be null or empty"));
    }

    var hasAccess = UsersWithAccess.Contains(username.Value.ToLowerInvariant());
    DataAccessLogs.UserAccessChecked(_logger, username.Value, hasAccess);

    return Task.FromResult(Result.Ok(hasAccess));
  }
}
