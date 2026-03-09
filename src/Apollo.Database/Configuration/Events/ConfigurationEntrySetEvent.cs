using Apollo.Database.Common;

namespace Apollo.Database.Configuration.Events;

public sealed record ConfigurationEntrySetEvent(string Key, string EncryptedValue, DateTime SetOn) : BaseEvent;
