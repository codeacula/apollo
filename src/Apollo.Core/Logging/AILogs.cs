using Microsoft.Extensions.Logging;

namespace Apollo.Core.Logging;

/// <summary>
/// High-performance logging definitions for AI operations.
/// EventIds: 3400-3499
/// </summary>
public static partial class AILogs
{
  [LoggerMessage(
    EventId = 3400,
    Level = LogLevel.Information,
    Message = "AI Request Started - ToolCalling: {ToolCallingEnabled}, Temperature: {Temperature}")]
  public static partial void AIRequestStarted(ILogger logger, bool toolCallingEnabled, double temperature);

  [LoggerMessage(
    EventId = 3401,
    Level = LogLevel.Debug,
    Message = "Calling LLM (setup took {SetupDurationMs}ms) - Messages: {MessageCount}, Plugins: {PluginCount}, ToolCalling: {ToolCallingEnabled}")]
  public static partial void LLMCallStarted(ILogger logger, double setupDurationMs, int messageCount, int pluginCount, bool toolCallingEnabled);

  [LoggerMessage(
    EventId = 3402,
    Level = LogLevel.Information,
    Message = "LLM Response Received (took {DurationMs:F0}ms with {ToolCallCount} tool calls)")]
  public static partial void LLMResponseReceived(ILogger logger, double durationMs, int toolCallCount);

  [LoggerMessage(
    EventId = 3403,
    Level = LogLevel.Warning,
    Message = "AI Request Terminated - Infinite loop detected, but {ToolCallCount} tool calls succeeded before termination")]
  public static partial void AIRequestTerminatedLoopDetected(ILogger logger, int toolCallCount);

  [LoggerMessage(
    EventId = 3404,
    Level = LogLevel.Error,
    Message = "AI Request Failed: {ExceptionType} - {ToolCallCount} tool calls succeeded before failure")]
  public static partial void AIRequestFailed(ILogger logger, string exceptionType, int toolCallCount, Exception exception);

  [LoggerMessage(
    EventId = 3405,
    Level = LogLevel.Debug,
    Message = "Tool Invocation Attempt - {PluginName}.{FunctionName}")]
  public static partial void ToolInvocationAttempt(ILogger logger, string pluginName, string functionName);

  [LoggerMessage(
    EventId = 3406,
    Level = LogLevel.Warning,
    Message = "Tool Invocation Blocked - {PluginName}.{FunctionName}: {Reason}")]
  public static partial void ToolInvocationBlocked(ILogger logger, string pluginName, string functionName, string reason);

  [LoggerMessage(
    EventId = 3407,
    Level = LogLevel.Debug,
    Message = "Tool Invocation Completed - {PluginName}.{FunctionName} in {DurationMs:F0}ms")]
  public static partial void ToolInvocationCompleted(ILogger logger, string pluginName, string functionName, double durationMs);

  [LoggerMessage(
    EventId = 3408,
    Level = LogLevel.Error,
    Message = "Infinite Loop Detected - {ToolCallKey} called {RepeatCount} times consecutively")]
  public static partial void InfiniteLoopDetected(ILogger logger, string toolCallKey, int repeatCount);
}
