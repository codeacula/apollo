using System.Reflection;

using Microsoft.SemanticKernel;

namespace Apollo.AI.Tooling;

public static class ToolCallResolver
{
  public static bool TryResolve(
    IDictionary<string, object> plugins,
    string pluginName,
    string functionName,
    out ResolvedToolCall resolved,
    out string errorMessage)
  {
    resolved = new ResolvedToolCall(null!, null!);
    errorMessage = "";

    if (string.IsNullOrWhiteSpace(pluginName))
    {
      errorMessage = "Plugin name is missing.";
      return false;
    }

    if (string.IsNullOrWhiteSpace(functionName))
    {
      errorMessage = "Function name is missing.";
      return false;
    }

    if (!plugins.TryGetValue(pluginName, out var plugin))
    {
      errorMessage = $"Unknown plugin '{pluginName}'.";
      return false;
    }

    var method = FindFunctionMethod(plugin, functionName);
    if (method is null)
    {
      errorMessage = $"Unknown function '{functionName}' for plugin '{pluginName}'.";
      return false;
    }

    resolved = new ResolvedToolCall(plugin, method);
    return true;
  }

  private static MethodInfo? FindFunctionMethod(object plugin, string functionName)
  {
    foreach (var method in plugin.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance))
    {
      var attribute = method.GetCustomAttribute<KernelFunctionAttribute>();
      if (attribute is null)
      {
        continue;
      }

      var name = attribute.Name ?? method.Name;
      if (string.Equals(name, functionName, StringComparison.OrdinalIgnoreCase))
      {
        return method;
      }
    }

    return null;
  }
}

public sealed record ResolvedToolCall(object Plugin, MethodInfo Method);
