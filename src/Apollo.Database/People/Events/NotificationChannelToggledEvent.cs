using Apollo.Domain.Common.Enums;

namespace Apollo.Database.People.Events;

public sealed record NotificationChannelToggledEvent(
  Platform PersonPlatform,
  string PersonProviderId,
  NotificationChannelType ChannelType,
  string Identifier,
  bool IsEnabled,
  DateTime ToggledOn);
