using Apollo.Domain.Users.ValueObjects;

using FluentResults;

namespace Apollo.Core.Infrastructure.Services;

public interface IUserValidationService
{
  Task<Result<bool>> ValidateUserAccessAsync(Username username, CancellationToken cancellationToken = default);
}
