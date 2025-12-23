using Apollo.Domain.Common.Enums;

namespace Apollo.Database.People.Events;

public sealed record NotificationChannelAddedEvent(
  Guid PersonId,
  NotificationChannelType ChannelType,
  string Identifier,
  DateTime AddedOn);
