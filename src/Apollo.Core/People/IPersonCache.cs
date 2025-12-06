using Apollo.Domain.People.ValueObjects;

using FluentResults;

namespace Apollo.Core.People;

/// <summary>
/// Provides caching operations for user access validation.
/// </summary>
public interface IPersonCache
{
  Task<Result<bool?>> GetAccessAsync(Username username);
  Task<Result> SetAccessAsync(Username username, bool hasAccess);
  Task<Result> InvalidateAccessAsync(Username username);
}
