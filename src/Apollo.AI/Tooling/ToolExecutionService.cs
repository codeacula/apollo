using System.Reflection;
using System.Text.Json;

using Apollo.AI.Models;
using Apollo.AI.Requests;

namespace Apollo.AI.Tooling;

public sealed class ToolExecutionService
{
  public static async Task<List<ToolCallResult>> ExecuteToolPlanAsync(
    IEnumerable<PlannedToolCall> toolCalls,
    IDictionary<string, object> plugins,
    CancellationToken cancellationToken)
  {
    var results = new List<ToolCallResult>();
    foreach (var toolCall in toolCalls)
    {
      var arguments = toolCall.Arguments ?? [];
      if (!ToolCallResolver.TryResolve(plugins, toolCall.PluginName, toolCall.FunctionName, out var resolved, out var errorMessage))
      {
        results.Add(new ToolCallResult
        {
          PluginName = toolCall.PluginName,
          FunctionName = toolCall.FunctionName,
          Arguments = SerializeArguments(arguments),
          Success = false,
          ErrorMessage = errorMessage
        });
        continue;
      }

      try
      {
        var result = await InvokeAsync(resolved.Plugin, resolved.Method, arguments, cancellationToken);
        results.Add(new ToolCallResult
        {
          PluginName = toolCall.PluginName,
          FunctionName = toolCall.FunctionName,
          Arguments = SerializeArguments(arguments),
          Result = result,
          Success = true
        });
      }
      catch (Exception ex)
      {
        results.Add(new ToolCallResult
        {
          PluginName = toolCall.PluginName,
          FunctionName = toolCall.FunctionName,
          Arguments = SerializeArguments(arguments),
          Success = false,
          ErrorMessage = ex.Message
        });
      }
    }

    return results;
  }

  private static async Task<string> InvokeAsync(
    object plugin,
    MethodInfo method,
    IDictionary<string, string?> arguments,
    CancellationToken cancellationToken)
  {
    var parameters = method.GetParameters();
    var values = new object?[parameters.Length];

    for (var i = 0; i < parameters.Length; i++)
    {
      var parameter = parameters[i];
      if (parameter.ParameterType == typeof(CancellationToken))
      {
        values[i] = cancellationToken;
        continue;
      }

      if (string.IsNullOrWhiteSpace(parameter.Name))
      {
        values[i] = parameter.HasDefaultValue ? parameter.DefaultValue : null;
        continue;
      }

      if (arguments.TryGetValue(parameter.Name, out var argumentValue))
      {
        values[i] = ConvertParameter(argumentValue, parameter.ParameterType);
        continue;
      }

      values[i] = parameter.HasDefaultValue ? parameter.DefaultValue : null;
    }

    var invocationResult = method.Invoke(plugin, values);
    return invocationResult switch
    {
      Task<string> stringTask => await stringTask,
      Task task => await FinishTaskAsync(task),
      string value => value,
      null => "",
      _ => invocationResult.ToString() ?? ""
    };
  }

  private static async Task<string> FinishTaskAsync(Task task)
  {
    await task;
    return "";
  }

  private static object? ConvertParameter(string? value, Type targetType)
  {
    if (targetType == typeof(string))
    {
      return value;
    }

    if (string.IsNullOrWhiteSpace(value))
    {
      return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
    }

    var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;
    return Convert.ChangeType(value, underlyingType, System.Globalization.CultureInfo.InvariantCulture);
  }

  private static string SerializeArguments(IDictionary<string, string?> arguments)
  {
    return JsonSerializer.Serialize(arguments);
  }
}
