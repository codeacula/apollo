namespace Apollo.Database.ToDos.Events;

public sealed record ToDoReminderCancelledEvent(
  Guid Id,
  DateTime CancelledOn);
