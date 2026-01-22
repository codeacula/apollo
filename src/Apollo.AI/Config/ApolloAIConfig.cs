namespace Apollo.AI.Config;

public record ApolloAIConfig
{
  public string ModelId { get; init; } = "";
  public string Endpoint { get; init; } = "";
  public string ApiKey { get; init; } = "";
}
