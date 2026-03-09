using Apollo.Database.Common;

namespace Apollo.Database.Configuration.Events;

public sealed record ConfigurationEntryDeletedEvent(string Key, DateTime DeletedOn) : BaseEvent;
