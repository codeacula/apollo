using Apollo.Domain.Common.ValueObjects;

namespace Apollo.Domain.Users.Models;

public record User()
{
  public required UserId Id { get; init; }
  public required DisplayName DisplayName { get; init; }

  public ICollection<Reminder> Reminders { get; init; } = [];
}
