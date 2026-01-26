using Apollo.Domain.People.ValueObjects;

using FluentResults;

namespace Apollo.Core.People;

/// <summary>
/// Provides caching operations for user access validation.
/// </summary>
public interface IPersonCache
{
  Task<Result<PersonId?>> GetPersonIdAsync(PlatformId platformId);
  Task<Result> MapPlatformIdToPersonIdAsync(PlatformId platformId, PersonId personId);
  Task<Result> InvalidatePlatformMappingAsync(PlatformId platformId);
  Task<Result<bool?>> GetAccessAsync(PlatformId platformId);
  Task<Result> SetAccessAsync(PersonId personId, bool hasAccess);
  Task<Result> InvalidateAccessAsync(PersonId personId);
}
