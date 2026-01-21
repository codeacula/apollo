using Apollo.AI.Config;
using Apollo.AI.DTOs;
using Apollo.AI.Prompts;
using Apollo.AI.Requests;

namespace Apollo.AI;

public sealed class ApolloAIAgent(ApolloAIConfig config, IPromptLoader promptLoader) : IApolloAIAgent
{
  private const string ToolCallingPromptName = "ApolloToolCalling";
  private const string ResponsePromptName = "ApolloResponse";

  public IAIRequestBuilder CreateRequest()
  {
    return new AIRequestBuilder(config);
  }

  public IAIRequestBuilder CreateToolCallingRequest(
    IEnumerable<ChatMessageDTO> messages,
    IDictionary<string, object> plugins)
  {
    var prompt = promptLoader.Load(ToolCallingPromptName);

    return CreateRequest()
      .FromPromptDefinition(prompt)
      .WithMessages(messages)
      .WithPlugins(plugins)
      .WithToolCalling(enabled: true);
  }

  public IAIRequestBuilder CreateResponseRequest(
    IEnumerable<ChatMessageDTO> messages,
    string actionsSummary)
  {
    var prompt = promptLoader.Load(ResponsePromptName);
    var systemPrompt = BuildResponseSystemPrompt(prompt.SystemPrompt, actionsSummary);

    return CreateRequest()
      .WithSystemPrompt(systemPrompt)
      .WithTemperature(prompt.Temperature)
      .WithMessages(messages)
      .WithToolCalling(enabled: false);
  }

  private static string BuildResponseSystemPrompt(string basePrompt, string actionsSummary)
  {
    return $"{basePrompt}\n\n# Actions Taken This Request\n\n{actionsSummary}";
  }
}
