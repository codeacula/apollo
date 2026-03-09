using Apollo.Database.Configuration.Events;
using JasperFx.Events;

namespace Apollo.Database.Configuration;

public sealed record DbConfigurationEntry
{
  public required string Id { get; init; } // Key
  public required string EncryptedValue { get; init; }
  public DateTime CreatedOn { get; init; }
  public DateTime UpdatedOn { get; init; }

  public static DbConfigurationEntry Create(IEvent<ConfigurationEntrySetEvent> ev)
  {
    return new DbConfigurationEntry
    {
      Id = ev.Data.Key,
      EncryptedValue = ev.Data.EncryptedValue,
      CreatedOn = ev.Data.SetOn,
      UpdatedOn = ev.Data.SetOn
    };
  }

  public static DbConfigurationEntry Apply(IEvent<ConfigurationEntrySetEvent> ev, DbConfigurationEntry current)
  {
    return current with
    {
      EncryptedValue = ev.Data.EncryptedValue,
      UpdatedOn = ev.Data.SetOn
    };
  }
}
