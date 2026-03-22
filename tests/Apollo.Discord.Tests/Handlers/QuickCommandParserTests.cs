using Apollo.Discord.Handlers;

namespace Apollo.Discord.Tests.Handlers;

public class QuickCommandParserTests
{
  public static TheoryData<string, bool> ToDoCommandCases => new()
  {
    { "todo Buy groceries", true },
    { "Todo Buy groceries", true },
    { "TODO Buy groceries", true },
    { "task Buy groceries", true },
    { "Task Buy groceries", true },
    { "TASK Buy groceries", true },
    { "  todo spaced", true },
    { "\ttask tabbed", true },
    { "todolist", false },
    { "tasklist", false },
    { "my todo", false },
    { "task", false },
    { "hello world", false },
    { "", false }
  };

  public static TheoryData<string, bool> ReminderCommandCases => new()
  {
    { "remind me to take a break in 10 minutes", true },
    { "Remind take a break in 10 minutes", true },
    { "REMIND me to call mom in 1 hour", true },
    { "reminder take a break in 30 minutes", true },
    { "REMINDER take a break in 30 minutes", true },
    { "remember something", false },
    { "hello world", false },
    { "", false }
  };

  [Theory]
  [MemberData(nameof(ToDoCommandCases))]
  public void IsToDoCommandReturnsExpectedResult(string content, bool expected)
  {
    var result = QuickCommandParser.IsToDoCommand(content);

    Assert.Equal(expected, result);
  }

  [Theory]
  [InlineData("todo Buy groceries", "Buy groceries")]
  [InlineData("task Call mom tomorrow", "Call mom tomorrow")]
  [InlineData("TODO   multiple spaces   ", "multiple spaces")]
  [InlineData("task single", "single")]
  public void TryParseToDoWithTodoPrefixReturnsDescription(string content, string expectedDescription)
  {
    var result = QuickCommandParser.TryParseToDo(content, out var description);

    Assert.True(result);
    Assert.Equal(expectedDescription, description);
  }

  [Theory]
  [InlineData("todo ")]
  [InlineData("task")]
  [InlineData("todo")]
  [InlineData("hello world")]
  public void TryParseToDoWithInvalidInputReturnsFalse(string content)
  {
    var result = QuickCommandParser.TryParseToDo(content, out var description);

    Assert.False(result);
    Assert.Empty(description);
  }

  [Theory]
  [MemberData(nameof(ReminderCommandCases))]
  public void IsReminderCommandReturnsExpectedResult(string content, bool expected)
  {
    var result = QuickCommandParser.IsReminderCommand(content);

    Assert.Equal(expected, result);
  }

  [Theory]
  [InlineData("remind take a break in 10 minutes", "take a break", "10 minutes")]
  [InlineData("remind me to call mom in 1 hour", "call mom", "1 hour")]
  [InlineData("remind check the oven in 30 minutes", "check the oven", "30 minutes")]
  [InlineData("reminder to stand up at 3pm", "stand up", "3pm")]
  [InlineData("remind me drink water in 2 hours", "drink water", "2 hours")]
  public void TryParseReminderWithValidInputReturnsMessageAndTime(string content, string expectedMessage, string expectedTime)
  {
    var result = QuickCommandParser.TryParseReminder(content, out var message, out var time);

    Assert.True(result);
    Assert.Equal(expectedMessage, message);
    Assert.Equal(expectedTime, time);
  }

  [Theory]
  [InlineData("remind")]
  [InlineData("remind me")]
  [InlineData("remind something")]
  [InlineData("hello world")]
  public void TryParseReminderWithInvalidInputReturnsFalse(string content)
  {
    var result = QuickCommandParser.TryParseReminder(content, out var message, out var time);

    Assert.False(result);
    Assert.Empty(message);
    Assert.Empty(time);
  }
}
