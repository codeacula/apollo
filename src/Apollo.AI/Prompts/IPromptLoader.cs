namespace Apollo.AI.Prompts;

public interface IPromptLoader
{
  PromptDefinition Load(string promptName);
  Task<PromptDefinition> LoadAsync(string promptName, CancellationToken ct = default);
}
