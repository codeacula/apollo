using Apollo.AI.DTOs;
using Apollo.AI.Prompts;

namespace Apollo.AI.Requests;

public interface IAIRequestBuilder
{
  IAIRequestBuilder WithSystemPrompt(string systemPrompt);
  IAIRequestBuilder WithMessages(IEnumerable<ChatMessageDTO> messages);
  IAIRequestBuilder WithMessage(ChatMessageDTO message);
  IAIRequestBuilder WithTemperature(double temperature);
  IAIRequestBuilder WithPlugin(string pluginName, object plugin);
  IAIRequestBuilder WithPlugins(IDictionary<string, object> plugins);
  IAIRequestBuilder WithToolCalling(bool enabled = true);
  IAIRequestBuilder WithJsonMode(bool enabled = true);
  IAIRequestBuilder FromPromptDefinition(PromptDefinition prompt);
  IAIRequestBuilder WithTemplateVariables(IDictionary<string, string> variables);

  Task<AIRequestResult> ExecuteAsync(CancellationToken cancellationToken = default);
}
