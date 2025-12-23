using Apollo.Domain.Common.Enums;

namespace Apollo.Database.People.Events;

public sealed record NotificationChannelToggledEvent(
  Guid PersonId,
  NotificationChannelType ChannelType,
  string Identifier,
  bool IsEnabled,
  DateTime ToggledOn);
