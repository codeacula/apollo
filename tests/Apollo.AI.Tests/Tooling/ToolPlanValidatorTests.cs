using Apollo.AI.DTOs;
using Apollo.AI.Enums;
using Apollo.AI.Models;
using Apollo.AI.Tooling;

using Microsoft.SemanticKernel;

namespace Apollo.AI.Tests.Tooling;

public class ToolPlanValidatorTests
{
  [Fact]
  public void ValidateBlocksSetTimezoneWithoutContext()
  {
    var plan = new ToolPlan
    {
      ToolCalls =
      [
        new PlannedToolCall
        {
          PluginName = "Person",
          FunctionName = "set_timezone",
          Arguments = new Dictionary<string, string?> { ["timezone"] = "PST" }
        }
      ]
    };

    var context = BuildContext(messages:
    [
      new ChatMessageDTO(ChatRole.User, "PST", DateTime.UtcNow)
    ]);

    var result = ToolPlanValidator.Validate(plan, context);

    Assert.Empty(result.ApprovedCalls);
    _ = Assert.Single(result.BlockedCalls);
  }

  [Fact]
  public void ValidateAllowsSetTimezoneWithContext()
  {
    var plan = new ToolPlan
    {
      ToolCalls =
      [
        new PlannedToolCall
        {
          PluginName = "Person",
          FunctionName = "set_timezone",
          Arguments = new Dictionary<string, string?> { ["timezone"] = "PST" }
        }
      ]
    };

    var context = BuildContext(messages:
    [
      new ChatMessageDTO(ChatRole.Assistant, "What timezone are you in?", DateTime.UtcNow.AddMinutes(-1)),
      new ChatMessageDTO(ChatRole.User, "PST", DateTime.UtcNow)
    ]);

    var result = ToolPlanValidator.Validate(plan, context);

    _ = Assert.Single(result.ApprovedCalls);
    Assert.Empty(result.BlockedCalls);
  }

  [Fact]
  public void ValidateBlocksMissingArguments()
  {
    var plan = new ToolPlan
    {
      ToolCalls =
      [
        new PlannedToolCall
        {
          PluginName = "ToDos",
          FunctionName = "create_todo",
          Arguments = []
        }
      ]
    };

    var context = BuildContext();
    var result = ToolPlanValidator.Validate(plan, context);

    Assert.Empty(result.ApprovedCalls);
    _ = Assert.Single(result.BlockedCalls);
  }

  [Fact]
  public void ValidateBlocksDeleteAfterCreate()
  {
    var plan = new ToolPlan
    {
      ToolCalls =
      [
        new PlannedToolCall
        {
          PluginName = "ToDos",
          FunctionName = "create_todo",
          Arguments = new Dictionary<string, string?> { ["description"] = "Pay rent" }
        },
        new PlannedToolCall
        {
          PluginName = "ToDos",
          FunctionName = "delete_todo",
          Arguments = new Dictionary<string, string?> { ["todoId"] = "123" }
        }
      ]
    };

    var context = BuildContext();
    var result = ToolPlanValidator.Validate(plan, context);

    _ = Assert.Single(result.ApprovedCalls);
    _ = Assert.Single(result.BlockedCalls);
  }

  [Fact]
  public void ValidateReturnsEmptyResultForEmptyPlan()
  {
    var plan = new ToolPlan { ToolCalls = [] };
    var context = BuildContext();

    var result = ToolPlanValidator.Validate(plan, context);

    Assert.Empty(result.ApprovedCalls);
    Assert.Empty(result.BlockedCalls);
  }

  [Fact]
  public void ValidateBlocksUnknownPlugin()
  {
    var plan = new ToolPlan
    {
      ToolCalls =
      [
        new PlannedToolCall
        {
          PluginName = "UnknownPlugin",
          FunctionName = "some_function",
          Arguments = new Dictionary<string, string?> { ["arg"] = "value" }
        }
      ]
    };

    var context = BuildContext();
    var result = ToolPlanValidator.Validate(plan, context);

    Assert.Empty(result.ApprovedCalls);
    _ = Assert.Single(result.BlockedCalls);
    Assert.Contains("Unknown plugin", result.BlockedCalls[0].ErrorMessage);
  }

  [Fact]
  public void ValidateBlocksUnknownFunction()
  {
    var plan = new ToolPlan
    {
      ToolCalls =
      [
        new PlannedToolCall
        {
          PluginName = "ToDos",
          FunctionName = "unknown_function",
          Arguments = new Dictionary<string, string?> { ["arg"] = "value" }
        }
      ]
    };

    var context = BuildContext();
    var result = ToolPlanValidator.Validate(plan, context);

    Assert.Empty(result.ApprovedCalls);
    _ = Assert.Single(result.BlockedCalls);
    Assert.Contains("Unknown function", result.BlockedCalls[0].ErrorMessage);
  }

  [Fact]
  public void ValidateBlocksExcessiveToolCalls()
  {
    var toolCalls = Enumerable.Range(1, 10)
      .Select(i => new PlannedToolCall
      {
        PluginName = "ToDos",
        FunctionName = i % 2 == 0 ? "create_todo" : "delete_todo",
        Arguments = new Dictionary<string, string?>
        {
          [i % 2 == 0 ? "description" : "todoId"] = $"Value {i}"
        }
      })
      .ToList();

    var plan = new ToolPlan { ToolCalls = toolCalls };
    var context = BuildContext();

    var result = ToolPlanValidator.Validate(plan, context);

    Assert.Equal(ToolCallMatchers.MaxToolCalls, result.ApprovedCalls.Count);
    Assert.True(result.BlockedCalls.Count >= 10 - ToolCallMatchers.MaxToolCalls);
  }

  [Fact]
  public void ValidateBlocksConsecutiveRepeatedCalls()
  {
    var toolCalls = Enumerable.Range(1, 5)
      .Select(_ => new PlannedToolCall
      {
        PluginName = "ToDos",
        FunctionName = "delete_todo",
        Arguments = new Dictionary<string, string?> { ["todoId"] = "123" }
      })
      .ToList();

    var plan = new ToolPlan { ToolCalls = toolCalls };
    var context = BuildContext();

    var result = ToolPlanValidator.Validate(plan, context);

    Assert.Equal(ToolCallMatchers.MaxConsecutiveRepeats, result.ApprovedCalls.Count);
    Assert.Equal(5 - ToolCallMatchers.MaxConsecutiveRepeats, result.BlockedCalls.Count);
    Assert.All(result.BlockedCalls, b => Assert.Contains("Repeated tool call", b.ErrorMessage));
  }

  [Fact]
  public void ValidateResetsRepeatCounterForDifferentCalls()
  {
    var plan = new ToolPlan
    {
      ToolCalls =
      [
        new PlannedToolCall
        {
          PluginName = "ToDos",
          FunctionName = "delete_todo",
          Arguments = new Dictionary<string, string?> { ["todoId"] = "1" }
        },
        new PlannedToolCall
        {
          PluginName = "ToDos",
          FunctionName = "create_todo",
          Arguments = new Dictionary<string, string?> { ["description"] = "New task" }
        },
        new PlannedToolCall
        {
          PluginName = "ToDos",
          FunctionName = "delete_todo",
          Arguments = new Dictionary<string, string?> { ["todoId"] = "2" }
        }
      ]
    };

    var context = BuildContext();
    var result = ToolPlanValidator.Validate(plan, context);

    Assert.Equal(2, result.ApprovedCalls.Count);
    _ = Assert.Single(result.BlockedCalls);
  }

  [Fact]
  public void ValidateBlocksCompleteAfterCreate()
  {
    var plan = new ToolPlan
    {
      ToolCalls =
      [
        new PlannedToolCall
        {
          PluginName = "ToDos",
          FunctionName = "create_todo",
          Arguments = new Dictionary<string, string?> { ["description"] = "New task" }
        },
        new PlannedToolCall
        {
          PluginName = "ToDos",
          FunctionName = "complete_todo",
          Arguments = new Dictionary<string, string?> { ["todoId"] = "123" }
        }
      ]
    };

    var context = BuildContext();
    var result = ToolPlanValidator.Validate(plan, context);

    _ = Assert.Single(result.ApprovedCalls);
    _ = Assert.Single(result.BlockedCalls);
    Assert.Contains("newly created", result.BlockedCalls[0].ErrorMessage);
  }

  [Fact]
  public void ValidateAllowsTimezoneWithTimeZoneKeyword()
  {
    var plan = new ToolPlan
    {
      ToolCalls =
      [
        new PlannedToolCall
        {
          PluginName = "Person",
          FunctionName = "set_timezone",
          Arguments = new Dictionary<string, string?> { ["timezone"] = "EST" }
        }
      ]
    };

    var context = BuildContext(messages:
    [
      new ChatMessageDTO(ChatRole.User, "Please set my time zone to EST", DateTime.UtcNow)
    ]);

    var result = ToolPlanValidator.Validate(plan, context);

    _ = Assert.Single(result.ApprovedCalls);
    Assert.Empty(result.BlockedCalls);
  }

  [Fact]
  public void ValidateAllowsTimezoneWithTzKeyword()
  {
    var plan = new ToolPlan
    {
      ToolCalls =
      [
        new PlannedToolCall
        {
          PluginName = "Person",
          FunctionName = "set_timezone",
          Arguments = new Dictionary<string, string?> { ["timezone"] = "PST" }
        }
      ]
    };

    var context = BuildContext(messages:
    [
      new ChatMessageDTO(ChatRole.User, "My tz is PST", DateTime.UtcNow)
    ]);

    var result = ToolPlanValidator.Validate(plan, context);

    _ = Assert.Single(result.ApprovedCalls);
    Assert.Empty(result.BlockedCalls);
  }

  [Fact]
  public void ValidateApprovesValidToolCallWithOptionalArguments()
  {
    var plan = new ToolPlan
    {
      ToolCalls =
      [
        new PlannedToolCall
        {
          PluginName = "ToDos",
          FunctionName = "create_todo",
          Arguments = new Dictionary<string, string?> { ["description"] = "Test task" }
        }
      ]
    };

    var context = BuildContext();
    var result = ToolPlanValidator.Validate(plan, context);

    _ = Assert.Single(result.ApprovedCalls);
    Assert.Empty(result.BlockedCalls);
  }

  [Fact]
  public void ValidateNormalizesArgumentsToCaseInsensitive()
  {
    var plan = new ToolPlan
    {
      ToolCalls =
      [
        new PlannedToolCall
        {
          PluginName = "ToDos",
          FunctionName = "create_todo",
          Arguments = new Dictionary<string, string?> { ["Description"] = "Test" }
        }
      ]
    };

    var context = BuildContext();
    var result = ToolPlanValidator.Validate(plan, context);

    _ = Assert.Single(result.ApprovedCalls);
    Assert.True(result.ApprovedCalls[0].Arguments.ContainsKey("description"));
  }

  private static ToolPlanValidationContext BuildContext(List<ChatMessageDTO>? messages = null)
  {
    var plugins = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
    {
      ["Person"] = new PersonPluginStub(),
      ["ToDos"] = new ToDoPluginStub()
    };

    return new ToolPlanValidationContext(
      plugins,
      messages ?? [],
      []);
  }

  private sealed class PersonPluginStub
  {
    [KernelFunction("set_timezone")]
    public static Task<string> SetTimeZoneAsync(string timezone)
    {
      return Task.FromResult(timezone);
    }
  }

  private sealed class ToDoPluginStub
  {
    [KernelFunction("create_todo")]
    public static Task<string> CreateToDoAsync(string description, string? reminderDate = null)
    {
      return Task.FromResult(description + reminderDate);
    }

    [KernelFunction("delete_todo")]
    public static Task<string> DeleteToDoAsync(string todoId)
    {
      return Task.FromResult(todoId);
    }

    [KernelFunction("complete_todo")]
    public static Task<string> CompleteToDoAsync(string todoId)
    {
      return Task.FromResult(todoId);
    }
  }
}
