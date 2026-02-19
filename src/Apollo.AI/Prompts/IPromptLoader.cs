namespace Apollo.AI.Prompts;

public interface IPromptLoader
{
  Task<PromptDefinition> LoadAsync(string promptName, CancellationToken ct = default);
}
