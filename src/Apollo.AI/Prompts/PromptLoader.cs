using System.Collections.Concurrent;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Apollo.AI.Prompts;

public sealed class PromptLoader(string? promptsDirectory = null) : IPromptLoader
{
  private readonly string _promptsDirectory = promptsDirectory
      ?? Path.Combine(AppContext.BaseDirectory, "Prompts");
  private readonly ConcurrentDictionary<string, PromptDefinition> _cache = new();
  private readonly IDeserializer _deserializer = new DeserializerBuilder()
      .WithNamingConvention(CamelCaseNamingConvention.Instance)
      .Build();

  public async Task<PromptDefinition> LoadAsync(string promptName, CancellationToken ct = default)
  {
    if (_cache.TryGetValue(promptName, out var cached))
    {
      return cached;
    }

    var filePath = Path.Combine(_promptsDirectory, $"{promptName}.yml");
    if (!File.Exists(filePath))
    {
      throw new FileNotFoundException($"Prompt file not found: {filePath}");
    }

    var yaml = await File.ReadAllTextAsync(filePath, ct);
    var definition = _deserializer.Deserialize<PromptDefinition>(yaml);

    _ = _cache.TryAdd(promptName, definition);
    return definition;
  }
}
