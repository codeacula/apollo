using Apollo.Domain.Common.ValueObjects;
using Apollo.Domain.Users.ValueObjects;

namespace Apollo.Domain.Users.Models;

public record User()
{
  public required UserId Id { get; init; }
  public required DisplayName DisplayName { get; init; }

  public required Username Username { get; init; }
}
