using Apollo.Domain.Common.ValueObjects;
using Apollo.Domain.Configuration.ValueObjects;

namespace Apollo.Domain.Configuration.Models;

public sealed record ApolloConfiguration
{
  public ConfigurationKey Key { get; init; }
  public SystemPrompt SystemPrompt { get; init; }
  public CreatedOn CreatedOn { get; init; }
  public UpdatedOn UpdatedOn { get; init; }
}
