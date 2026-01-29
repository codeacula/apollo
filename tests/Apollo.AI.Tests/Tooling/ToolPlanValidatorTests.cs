using Apollo.AI.DTOs;
using Apollo.AI.Enums;
using Apollo.AI.Models;
using Apollo.AI.Tooling;

using Microsoft.SemanticKernel;

namespace Apollo.AI.Tests.Tooling;

public class ToolPlanValidatorTests
{
  private readonly ToolPlanValidator _validator = new();

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
  }
}
