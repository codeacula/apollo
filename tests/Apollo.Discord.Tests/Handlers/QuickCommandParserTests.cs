using Apollo.Discord.Handlers;

namespace Apollo.Discord.Tests.Handlers;

public class QuickCommandParserTests
{
  [Theory]
  [InlineData("todo Buy groceries", true)]
  [InlineData("Todo Buy groceries", true)]
  [InlineData("TODO Buy groceries", true)]
  [InlineData("task Buy groceries", true)]
  [InlineData("Task Buy groceries", true)]
  [InlineData("TASK Buy groceries", true)]
  [InlineData("todolist", false)]
  [InlineData("tasklist", false)]
  [InlineData("my todo", false)]
  [InlineData("hello world", false)]
  [InlineData("", false)]
  public void IsToDoCommandDetectsToDoCommands(string content, bool expected)
  {
    var result = QuickCommandParser.IsToDoCommand(content);
    Assert.Equal(expected, result);
  }

  [Theory]
  [InlineData("remind me to take a break in 10 minutes", true)]
  [InlineData("Remind take a break in 10 minutes", true)]
  [InlineData("REMIND me to call mom in 1 hour", true)]
  [InlineData("reminder take a break in 30 minutes", true)]
  [InlineData("remember something", false)]
  [InlineData("hello world", false)]
  [InlineData("", false)]
  public void IsReminderCommandDetectsReminderCommands(string content, bool expected)
  {
    var result = QuickCommandParser.IsReminderCommand(content);
    Assert.Equal(expected, result);
  }

  [Theory]
  [InlineData("todo Buy groceries", "Buy groceries")]
  [InlineData("task Call mom tomorrow", "Call mom tomorrow")]
  [InlineData("TODO   multiple spaces   ", "multiple spaces")]
  [InlineData("task single", "single")]
  public void TryParseToDoExtractsDescription(string content, string expectedDescription)
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
  public void TryParseToDoReturnsFalseForInvalidInput(string content)
  {
    var result = QuickCommandParser.TryParseToDo(content, out var description);

    Assert.False(result);
    Assert.Empty(description);
  }

  [Theory]
  [InlineData("remind take a break in 10 minutes", "take a break", "10 minutes")]
  [InlineData("remind me to call mom in 1 hour", "call mom", "1 hour")]
  [InlineData("remind check the oven in 30 minutes", "check the oven", "30 minutes")]
  [InlineData("reminder to stand up at 3pm", "stand up", "3pm")]
  [InlineData("remind me drink water in 2 hours", "drink water", "2 hours")]
  public void TryParseReminderExtractsMessageAndTime(string content, string expectedMessage, string expectedTime)
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
  public void TryParseReminderReturnsFalseForInvalidInput(string content)
  {
    var result = QuickCommandParser.TryParseReminder(content, out var message, out var time);

    Assert.False(result);
    Assert.Empty(message);
    Assert.Empty(time);
  }

  [Theory]
  [InlineData("  todo spaced", true)]
  [InlineData("\ttodo tabbed", true)]
  public void IsToDoCommandHandlesLeadingWhitespace(string content, bool expected)
  {
    var result = QuickCommandParser.IsToDoCommand(content);
    Assert.Equal(expected, result);
  }
}
