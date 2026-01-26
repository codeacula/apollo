namespace Apollo.Database.ToDos.Events;

public sealed record ReminderAcknowledgedEvent(
  Guid Id,
  DateTime AcknowledgedOn);
