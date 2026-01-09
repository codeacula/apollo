using Apollo.Domain.Common.Enums;

namespace Apollo.Database.ToDos.Events;

public sealed record ToDoCreatedEvent(
  Guid Id,
  Platform PersonPlatform,
  string PersonProviderId,
  string Description,
  DateTime CreatedOn);
