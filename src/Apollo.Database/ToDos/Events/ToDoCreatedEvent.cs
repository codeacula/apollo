using Apollo.Domain.People.ValueObjects;

namespace Apollo.Database.ToDos.Events;

public sealed record ToDoCreatedEvent(
  Guid Id,
  PersonId PersonId,
  string Description,
  DateTime CreatedOn);
