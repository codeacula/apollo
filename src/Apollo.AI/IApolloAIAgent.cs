using Apollo.AI.DTOs;

namespace Apollo.AI;

public interface IApolloAIAgent
{
  Task<string> ChatAsync(ChatCompletionRequest chatCompletionRequest, CancellationToken cancellationToken = default);
  void AddPlugin(object plugin, string pluginName);
}
