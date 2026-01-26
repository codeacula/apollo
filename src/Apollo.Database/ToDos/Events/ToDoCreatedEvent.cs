namespace Apollo.Database.ToDos.Events;

public sealed record ToDoCreatedEvent(
  Guid Id,
  Guid PersonId,
  string Description,
  DateTime CreatedOn);
