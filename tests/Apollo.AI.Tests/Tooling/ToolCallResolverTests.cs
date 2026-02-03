using Apollo.AI.Tooling;

using Microsoft.SemanticKernel;

namespace Apollo.AI.Tests.Tooling;

public class ToolCallResolverTests
{
  [Fact]
  public void TryResolveReturnsFalseForEmptyPluginName()
  {
    var plugins = new Dictionary<string, object>();

    var result = ToolCallResolver.TryResolve(plugins, "", "test", out _, out var errorMessage);

    Assert.False(result);
    Assert.Equal("Plugin name is missing.", errorMessage);
  }

  [Fact]
  public void TryResolveReturnsFalseForWhitespacePluginName()
  {
    var plugins = new Dictionary<string, object>();

    var result = ToolCallResolver.TryResolve(plugins, "   ", "test", out _, out var errorMessage);

    Assert.False(result);
    Assert.Equal("Plugin name is missing.", errorMessage);
  }

  [Fact]
  public void TryResolveReturnsFalseForEmptyFunctionName()
  {
    var plugins = new Dictionary<string, object> { ["TestPlugin"] = new TestPluginStub() };

    var result = ToolCallResolver.TryResolve(plugins, "TestPlugin", "", out _, out var errorMessage);

    Assert.False(result);
    Assert.Equal("Function name is missing.", errorMessage);
  }

  [Fact]
  public void TryResolveReturnsFalseForWhitespaceFunctionName()
  {
    var plugins = new Dictionary<string, object> { ["TestPlugin"] = new TestPluginStub() };

    var result = ToolCallResolver.TryResolve(plugins, "TestPlugin", "   ", out _, out var errorMessage);

    Assert.False(result);
    Assert.Equal("Function name is missing.", errorMessage);
  }

  [Fact]
  public void TryResolveReturnsFalseForUnknownPlugin()
  {
    var plugins = new Dictionary<string, object>();

    var result = ToolCallResolver.TryResolve(plugins, "UnknownPlugin", "test", out _, out var errorMessage);

    Assert.False(result);
    Assert.Equal("Unknown plugin 'UnknownPlugin'.", errorMessage);
  }

  [Fact]
  public void TryResolveReturnsFalseForUnknownFunction()
  {
    var plugins = new Dictionary<string, object> { ["TestPlugin"] = new TestPluginStub() };

    var result = ToolCallResolver.TryResolve(plugins, "TestPlugin", "unknown_function", out _, out var errorMessage);

    Assert.False(result);
    Assert.Equal("Unknown function 'unknown_function' for plugin 'TestPlugin'.", errorMessage);
  }

  [Fact]
  public void TryResolveReturnsTrueForValidPluginAndFunction()
  {
    var plugins = new Dictionary<string, object> { ["TestPlugin"] = new TestPluginStub() };

    var result = ToolCallResolver.TryResolve(plugins, "TestPlugin", "test_function", out var resolved, out var errorMessage);

    Assert.True(result);
    Assert.Empty(errorMessage);
    Assert.NotNull(resolved.Plugin);
    Assert.NotNull(resolved.Method);
    Assert.Equal("TestFunctionAsync", resolved.Method.Name);
  }

  [Fact]
  public void TryResolveIsCaseInsensitiveForPluginName()
  {
    var plugins = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
    {
      ["TestPlugin"] = new TestPluginStub()
    };

    var result = ToolCallResolver.TryResolve(plugins, "testplugin", "test_function", out var resolved, out _);

    Assert.True(result);
    Assert.NotNull(resolved.Method);
  }

  [Fact]
  public void TryResolveIsCaseInsensitiveForFunctionName()
  {
    var plugins = new Dictionary<string, object> { ["TestPlugin"] = new TestPluginStub() };

    var result = ToolCallResolver.TryResolve(plugins, "TestPlugin", "TEST_FUNCTION", out var resolved, out _);

    Assert.True(result);
    Assert.NotNull(resolved.Method);
  }

  [Fact]
  public void TryResolveUsesKernelFunctionAttributeName()
  {
    var plugins = new Dictionary<string, object> { ["TestPlugin"] = new TestPluginStub() };

    var result = ToolCallResolver.TryResolve(plugins, "TestPlugin", "custom_name", out var resolved, out _);

    Assert.True(result);
    Assert.Equal("MethodWithCustomName", resolved.Method.Name);
  }

  private sealed class TestPluginStub
  {
    [KernelFunction("test_function")]
    public static Task<string> TestFunctionAsync(string input)
    {
      return Task.FromResult(input);
    }

    [KernelFunction("custom_name")]
    public static Task<string> MethodWithCustomNameAsync()
    {
      return Task.FromResult("result");
    }
  }
}
