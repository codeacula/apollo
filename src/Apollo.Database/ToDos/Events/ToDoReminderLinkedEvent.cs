namespace Apollo.Database.ToDos.Events;

public sealed record ToDoReminderLinkedEvent(
  Guid Id,
  Guid ToDoId,
  Guid ReminderId,
  DateTime LinkedOn);
