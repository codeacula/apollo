using Apollo.Core.Configuration;
using Apollo.Database.Configuration.Events;
using Apollo.Domain.Configuration.Models;
using FluentResults;
using Marten;

namespace Apollo.Database.Configuration;

public class ConfigurationStore(
  IDocumentSession session,
  ISecretProtector secretProtector) : IConfigurationStore
{
  public async Task<Result<ConfigurationEntry>> GetConfigurationAsync(string key, CancellationToken cancellationToken = default)
  {
    try
    {
      var entry = await session.Events.AggregateStreamAsync<DbConfigurationEntry>(key, token: cancellationToken);
      if (entry is null)
      {
        return Result.Fail($"Configuration entry for '{key}' not found.");
      }

      return Result.Ok(new ConfigurationEntry
      {
        Key = entry.Id,
        Value = secretProtector.Unprotect(entry.EncryptedValue),
        UpdatedOn = entry.UpdatedOn
      });
    }
    catch (Exception ex)
    {
      return Result.Fail(new Error("Failed to retrieve configuration").CausedBy(ex));
    }
  }

  public async Task<Result<IEnumerable<ConfigurationEntry>>> GetAllConfigurationsAsync(CancellationToken cancellationToken = default)
  {
    try
    {
      var dbEntries = await session.Query<DbConfigurationEntry>().ToListAsync(cancellationToken);
      var entries = dbEntries.Select(e => new ConfigurationEntry
      {
        Key = e.Id,
        Value = secretProtector.Unprotect(e.EncryptedValue),
        UpdatedOn = e.UpdatedOn
      }).ToList();

      return Result.Ok<IEnumerable<ConfigurationEntry>>(entries);
    }
    catch (Exception ex)
    {
      return Result.Fail(new Error("Failed to retrieve all configurations").CausedBy(ex));
    }
  }

  public async Task<Result> SetConfigurationAsync(string key, string value, CancellationToken cancellationToken = default)
  {
    try
    {
      var encryptedValue = secretProtector.Protect(value);
      var ev = new ConfigurationEntrySetEvent(key, encryptedValue, DateTime.UtcNow)
      {
        Id = Guid.NewGuid(),
        CreatedOn = DateTime.UtcNow
      };
      
      session.Events.StartStream<DbConfigurationEntry>(key, ev);
      await session.SaveChangesAsync(cancellationToken);
      
      return Result.Ok();
    }
    catch (Exception ex)
    {
      return Result.Fail(new Error("Failed to save configuration").CausedBy(ex));
    }
  }

  public async Task<Result> DeleteConfigurationAsync(string key, CancellationToken cancellationToken = default)
  {
    try
    {
      var ev = new ConfigurationEntryDeletedEvent(key, DateTime.UtcNow)
      {
        Id = Guid.NewGuid(),
        CreatedOn = DateTime.UtcNow
      };
      session.Events.Append(key, ev);
      await session.SaveChangesAsync(cancellationToken);
      return Result.Ok();
    }
    catch (Exception ex)
    {
      return Result.Fail(new Error("Failed to delete configuration").CausedBy(ex));
    }
  }
}
