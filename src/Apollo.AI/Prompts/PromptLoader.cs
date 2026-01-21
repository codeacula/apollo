using System.Collections.Concurrent;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Apollo.AI.Prompts;

public sealed class PromptLoader : IPromptLoader
{
  private readonly string _promptsDirectory;
  private readonly ConcurrentDictionary<string, PromptDefinition> _cache = new();
  private readonly IDeserializer _deserializer;

  public PromptLoader(string? promptsDirectory = null)
  {
    _promptsDirectory = promptsDirectory
      ?? Path.Combine(AppContext.BaseDirectory, "Prompts");

    _deserializer = new DeserializerBuilder()
      .WithNamingConvention(CamelCaseNamingConvention.Instance)
      .Build();
  }

  public PromptDefinition Load(string promptName)
  {
    return _cache.GetOrAdd(promptName, name =>
    {
      var filePath = Path.Combine(_promptsDirectory, $"{name}.yml");
      if (!File.Exists(filePath))
      {
        throw new FileNotFoundException($"Prompt file not found: {filePath}");
      }

      var yaml = File.ReadAllText(filePath);
      return _deserializer.Deserialize<PromptDefinition>(yaml);
    });
  }

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

    _cache.TryAdd(promptName, definition);
    return definition;
  }
}
