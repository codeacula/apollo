using Apollo.Domain.Common.Enums;

namespace Apollo.Database.People.Events;

public sealed record NotificationChannelAddedEvent(
  Platform PersonPlatform,
  string PersonProviderId,
  NotificationChannelType ChannelType,
  string Identifier,
  DateTime AddedOn);
