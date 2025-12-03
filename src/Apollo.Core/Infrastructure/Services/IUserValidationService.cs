using Apollo.Domain.ValueObjects;

using FluentResults;

namespace Apollo.Core.Infrastructure.Services;

/// <summary>
/// Service for validating user access with caching.
/// </summary>
public interface IUserValidationService
{
  /// <summary>
  /// Validates whether a user has access to the system.
  /// Checks cache first, then falls back to data access layer.
  /// </summary>
  /// <param name="username">The username to validate.</param>
  /// <param name="cancellationToken">Cancellation token.</param>
  /// <returns>Result with true if user has access, false if denied, or failure on error.</returns>
  Task<Result<bool>> ValidateUserAccessAsync(Username username, CancellationToken cancellationToken = default);
}
