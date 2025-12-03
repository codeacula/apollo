using Apollo.Domain.Users.ValueObjects;

using FluentResults;

namespace Apollo.Core.Infrastructure.Data;

public interface IUserDataAccess
{
  Task<Result<bool>> GetUserAccessAsync(Username username, CancellationToken cancellationToken = default);
}
