using Apollo.Domain.People.ValueObjects;

using FluentResults;

namespace Apollo.Core.People;

/// <summary>
/// Provides caching operations for user access validation.
/// </summary>
public interface IPersonCache
{
  Task<Result<bool?>> GetAccessAsync(PlatformId platformId);
  Task<Result> SetAccessAsync(PlatformId platformId, bool hasAccess);
  Task<Result> InvalidateAccessAsync(PlatformId platformId);
}
