using Apollo.Domain.Configuration.Models;
using Apollo.Domain.Configuration.ValueObjects;

using FluentResults;

namespace Apollo.Core.Configuration;

public interface IApolloConfigurationStore
{
  Task<Result<ApolloConfiguration>> GetAsync(ConfigurationKey key, CancellationToken cancellationToken = default);
  Task<Result<ApolloConfiguration>> SetSystemPromptAsync(ConfigurationKey key, SystemPrompt systemPrompt, CancellationToken cancellationToken = default);
}
