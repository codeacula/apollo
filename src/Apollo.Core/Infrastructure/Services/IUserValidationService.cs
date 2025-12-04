using Apollo.Domain.Users.ValueObjects;

using FluentResults;

namespace Apollo.Core.Infrastructure.Services;

public interface IUserValidationService
{
  Task<Result<bool>> UserHasAccessAsync(Username username, CancellationToken cancellationToken = default);
}
