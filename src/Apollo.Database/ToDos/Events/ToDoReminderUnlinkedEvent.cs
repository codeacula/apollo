namespace Apollo.Database.ToDos.Events;

public sealed record ToDoReminderUnlinkedEvent(
  Guid Id,
  Guid ToDoId,
  Guid ReminderId,
  DateTime UnlinkedOn);
