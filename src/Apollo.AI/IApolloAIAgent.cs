using Apollo.AI.DTOs;

namespace Apollo.AI;

public interface IApolloAIAgent
{
  Task<string> ChatAsync(ChatCompletionRequestDTO chatCompletionRequest, CancellationToken cancellationToken = default);
  void AddPlugin(object plugin, string pluginName);
  void AddPlugin<TPluginType>(string pluginName);
}
