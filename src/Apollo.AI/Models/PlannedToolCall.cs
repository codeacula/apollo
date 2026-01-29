using System.Text.Json.Serialization;

namespace Apollo.AI.Models;

public sealed record PlannedToolCall
{
  [JsonPropertyName("plugin_name")]
  public string PluginName { get; init; } = "";

  [JsonPropertyName("function_name")]
  public string FunctionName { get; init; } = "";

  [JsonPropertyName("arguments")]
  public Dictionary<string, string?> Arguments { get; init; } = [];
}
