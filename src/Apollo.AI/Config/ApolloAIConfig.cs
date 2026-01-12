namespace Apollo.AI.Config;

public record ApolloAIConfig()
{
  public string ModelId { get; init; } = "";
  public string Endpoint { get; init; } = "";
  public string ApiKey { get; init; } = "";
  public string SystemPrompt { get; init; } = "";
  public string ToolCallingSystemPrompt { get; init; } = "";
  public string ResponseSystemPrompt { get; init; } = "";
  public double ToolCallingTemperature { get; init; } = 0.1;
  public double ResponseTemperature { get; init; } = 0.8;
}

