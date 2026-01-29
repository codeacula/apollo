namespace Apollo.Database.ToDos.Events;

public sealed record ToDoInterestUpdatedEvent(
  Guid Id,
  int Interest,
  DateTime UpdatedOn
);
