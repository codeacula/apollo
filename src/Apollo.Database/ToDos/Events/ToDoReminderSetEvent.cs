namespace Apollo.Database.ToDos.Events;

public sealed record ToDoReminderSetEvent(
  Guid Id,
  DateTime ReminderDate,
  DateTime SetOn);
