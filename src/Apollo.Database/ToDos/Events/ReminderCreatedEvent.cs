namespace Apollo.Database.ToDos.Events;

public sealed record ReminderCreatedEvent(
  Guid Id,
  string Details,
  DateTime ReminderTime,
  Guid QuartzJobId,
  DateTime CreatedOn);
