using Apollo.Domain.Common.Enums;

namespace Apollo.Database.People.Events;

public sealed record NotificationChannelRemovedEvent(
  Guid PersonId,
  NotificationChannelType ChannelType,
  string Identifier,
  DateTime RemovedOn);
