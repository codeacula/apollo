using Apollo.Domain.Common.ValueObjects;
using Apollo.Domain.Users.ValueObjects;

namespace Apollo.Domain.Users.Models;

public sealed record User
{
  public UserId Id { get; init; }
  public Username Username { get; init; }
  public HasAccess HasAccess { get; init; }
  public CreatedOn CreatedOn { get; init; }
  public UpdatedOn UpdatedOn { get; init; }
}
