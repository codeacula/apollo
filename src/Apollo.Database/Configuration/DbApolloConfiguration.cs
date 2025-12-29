using Apollo.Database.Configuration.Events;
using Apollo.Domain.Configuration.Models;
using Apollo.Domain.Configuration.ValueObjects;

using JasperFx.Events;

namespace Apollo.Database.Configuration;

public sealed record DbApolloConfiguration
{
  public required string Key { get; init; }
  public required string SystemPrompt { get; init; }
  public DateTime CreatedOn { get; init; }
  public DateTime UpdatedOn { get; init; }

  public static explicit operator ApolloConfiguration(DbApolloConfiguration config)
  {
    return new()
    {
      Key = new(config.Key),
      SystemPrompt = new(config.SystemPrompt),
      CreatedOn = new(config.CreatedOn),
      UpdatedOn = new(config.UpdatedOn)
    };
  }

  public static DbApolloConfiguration Create(IEvent<ConfigurationCreatedEvent> ev)
  {
    var eventData = ev.Data;

    return new()
    {
      Key = eventData.Key,
      SystemPrompt = eventData.SystemPrompt,
      CreatedOn = eventData.CreatedOn,
      UpdatedOn = eventData.CreatedOn
    };
  }

  public static DbApolloConfiguration Apply(IEvent<SystemPromptUpdatedEvent> ev, DbApolloConfiguration config)
  {
    return config with
    {
      SystemPrompt = ev.Data.SystemPrompt,
      UpdatedOn = ev.Data.UpdatedOn
    };
  }
}
