using Apollo.AI.Planning;

namespace Apollo.AI.Tests.Planning;

public sealed class ToolPlanParserTests
{
  #region Parse Valid JSON Tests

  [Fact]
  public void ParseWithValidJsonReturnsToolPlan()
  {
    // Arrange
    const string json = /*lang=json,strict*/ """
    {
      "tool_calls": [
        {
          "plugin_name": "ToDoPlugin",
          "function_name": "CreateToDo",
          "arguments": { "title": "Buy milk", "priority": "High" }
        }
      ]
    }
    """;

    // Act
    var result = ToolPlanParser.Parse(json);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
    _ = Assert.Single(result.Value.ToolCalls);
    Assert.Equal("ToDoPlugin", result.Value.ToolCalls[0].PluginName);
    Assert.Equal("CreateToDo", result.Value.ToolCalls[0].FunctionName);
  }

  [Fact]
  public void ParseWithMultipleToolCallsReturnsAll()
  {
    // Arrange
    const string json = /*lang=json,strict*/ """
    {
      "tool_calls": [
        {
          "plugin_name": "ToDoPlugin",
          "function_name": "CreateToDo",
          "arguments": { "title": "Task 1" }
        },
        {
          "plugin_name": "RemindersPlugin",
          "function_name": "CreateReminder",
          "arguments": { "text": "Reminder 1" }
        }
      ]
    }
    """;

    // Act
    var result = ToolPlanParser.Parse(json);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(2, result.Value.ToolCalls.Count);
  }

  [Fact]
  public void ParseWithCaseInsensitivePropertiesReturnsSuccess()
  {
    // Arrange
    const string json = /*lang=json,strict*/ """
    {
      "TOOL_CALLS": [
        {
          "PLUGIN_NAME": "ToDoPlugin",
          "FUNCTION_NAME": "CreateToDo",
          "ARGUMENTS": {}
        }
      ]
    }
    """;

    // Act
    var result = ToolPlanParser.Parse(json);

    // Assert
    Assert.True(result.IsSuccess);
    _ = Assert.Single(result.Value.ToolCalls);
  }

  [Fact]
  public void ParseWithEmptyToolCallsReturnsEmptyPlan()
  {
    // Arrange
    const string json = /*lang=json,strict*/ """
    {
      "tool_calls": []
    }
    """;

    // Act
    var result = ToolPlanParser.Parse(json);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Empty(result.Value.ToolCalls);
  }

  #endregion Parse Valid JSON Tests

  #region Parse Invalid/Malformed JSON Tests

  [Fact]
  public void ParseWithInvalidJsonReturnsFailed()
  {
    // Arrange
    const string json = "{ invalid json }";

    // Act
    var result = ToolPlanParser.Parse(json);

    // Assert
    Assert.True(result.IsFailed);
    Assert.NotEmpty(result.Errors);
  }

  [Fact]
  public void ParseWithMalformedJsonIncludesErrorMessage()
  {
    // Arrange
    const string json = """
    {
      "tool_calls": [
        {
          "plugin_name": "ToDoPlugin",
          "function_name": "CreateToDo"
        ]
    }
    """;

    // Act
    var result = ToolPlanParser.Parse(json);

    // Assert
    Assert.True(result.IsFailed);
    Assert.Contains("Invalid tool plan JSON", result.Errors[0].Message);
  }

  [Fact]
  public void ParseWithNullContentReturnsEmptyPlan()
  {
    // Act
    var result = ToolPlanParser.Parse(null);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Empty(result.Value.ToolCalls);
  }

  [Fact]
  public void ParseWithWhitespaceOnlyReturnsEmptyPlan()
  {
    // Act
    var result = ToolPlanParser.Parse("   ");

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Empty(result.Value.ToolCalls);
  }

  [Fact]
  public void ParseWithEmptyStringReturnsEmptyPlan()
  {
    // Act
    var result = ToolPlanParser.Parse("");

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Empty(result.Value.ToolCalls);
  }

  #endregion Parse Invalid/Malformed JSON Tests
}

