using Apollo.Core.Infrastructure.Data;
using Apollo.Core.Logging;
using Apollo.Domain.Users.ValueObjects;

using FluentResults;

using Marten;

using Microsoft.Extensions.Logging;

namespace Apollo.Database.Repository;

public sealed class MartenUserDataAccess(IDocumentStore documentStore, ILogger<MartenUserDataAccess> logger) : IUserDataAccess
{
  private readonly IDocumentStore _documentStore = documentStore;
  private readonly ILogger<MartenUserDataAccess> _logger = logger;

  public async Task<Result<bool>> GetUserAccessAsync(Username username, CancellationToken cancellationToken = default)
  {
    if (string.IsNullOrWhiteSpace(username.Value))
    {
      DataAccessLogs.UserNotFound(_logger, username.Value);
      return Result.Fail<bool>("Username cannot be null or empty");
    }

    await using var session = _documentStore.QuerySession();

    var user = await session.Query<UserReadModel>()
      .FirstOrDefaultAsync(u => u.Username == username.Value, cancellationToken);

    if (user == null)
    {
      DataAccessLogs.UserNotFound(_logger, username.Value);
      return Result.Fail<bool>($"User '{username.Value}' not found");
    }

    DataAccessLogs.UserAccessChecked(_logger, username.Value, user.HasAccess);
    return Result.Ok(user.HasAccess);
  }
}
