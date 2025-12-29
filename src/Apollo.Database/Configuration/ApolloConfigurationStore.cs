using Apollo.Core;
using Apollo.Core.Configuration;
using Apollo.Database.Configuration.Events;
using Apollo.Domain.Configuration.Models;
using Apollo.Domain.Configuration.ValueObjects;

using FluentResults;

using Marten;

namespace Apollo.Database.Configuration;

public sealed class ApolloConfigurationStore(IDocumentSession session, TimeProvider timeProvider) : IApolloConfigurationStore
{
  public async Task<Result<ApolloConfiguration>> GetAsync(ConfigurationKey key, CancellationToken cancellationToken = default)
  {
    try
    {
      var dbConfig = await session.Query<DbApolloConfiguration>().FirstOrDefaultAsync(c => c.Key == key.Value, cancellationToken);
      return dbConfig is not null ? Result.Ok((ApolloConfiguration)dbConfig) : Result.Fail<ApolloConfiguration>($"Configuration with key '{key.Value}' not found in database. Application will use fallback configuration from environment variables.");
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }

  public async Task<Result<ApolloConfiguration>> SetSystemPromptAsync(ConfigurationKey key, SystemPrompt systemPrompt, CancellationToken cancellationToken = default)
  {
    try
    {
      var time = timeProvider.GetUtcDateTime();
      var dbConfig = await session.Query<DbApolloConfiguration>().FirstOrDefaultAsync(c => c.Key == key.Value, cancellationToken);

      if (dbConfig is null)
      {
        var createEvent = new ConfigurationCreatedEvent(key.Value, systemPrompt.Value, time);
        _ = session.Events.StartStream<DbApolloConfiguration>(key.Value, createEvent);
      }
      else
      {
        var updateEvent = new SystemPromptUpdatedEvent(key.Value, systemPrompt.Value, time);
        _ = session.Events.Append(key.Value, updateEvent);
      }

      await session.SaveChangesAsync(cancellationToken);

      var updatedConfig = await session.Events.AggregateStreamAsync<DbApolloConfiguration>(key.Value, token: cancellationToken);
      return updatedConfig is null ? Result.Fail<ApolloConfiguration>($"Failed to retrieve updated configuration for key '{key.Value}' after save operation. The configuration may have been saved but could not be read back.") : Result.Ok((ApolloConfiguration)updatedConfig);
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }
}
