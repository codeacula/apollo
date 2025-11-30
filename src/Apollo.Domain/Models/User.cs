using Apollo.Domain.ValueObjects;

namespace Apollo.Domain.Models;

public record User()
{
  public required UserId Id { get; init; }
  public required DisplayName DisplayName { get; init; }

  public ICollection<Reminder> Reminders { get; init; } = [];
}
