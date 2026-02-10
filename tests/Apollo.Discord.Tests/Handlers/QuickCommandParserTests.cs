using Apollo.Discord.Handlers;

namespace Apollo.Discord.Tests.Handlers;

public class QuickCommandParserTests
{
  [Theory]
  [InlineData("todo Buy groceries")]
  [InlineData("Todo Buy groceries")]
  [InlineData("TODO Buy groceries")]
  public void IsToDoCommandWithTodoPrefixReturnsTrue(string content)
  {
    var result = QuickCommandParser.IsToDoCommand(content);

    Assert.True(result);
  }

  [Theory]
  [InlineData("task Buy groceries")]
  [InlineData("Task Buy groceries")]
  [InlineData("TASK Buy groceries")]
  public void IsToDoCommandWithTaskPrefixReturnsTrue(string content)
  {
    var result = QuickCommandParser.IsToDoCommand(content);

    Assert.True(result);
  }

  [Theory]
  [InlineData("todolist")]
  [InlineData("tasklist")]
  [InlineData("my todo")]
  [InlineData("task")]
  [InlineData("hello world")]
  [InlineData("")]
  public void IsToDoCommandWithNonCommandReturnsFalse(string content)
  {
    var result = QuickCommandParser.IsToDoCommand(content);

    Assert.False(result);
  }

  [Theory]
  [InlineData("  todo spaced")]
  [InlineData("\ttask tabbed")]
  public void IsToDoCommandWithLeadingWhitespaceReturnsTrue(string content)
  {
    var result = QuickCommandParser.IsToDoCommand(content);

    Assert.True(result);
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
  [InlineData("remind me to take a break in 10 minutes")]
  [InlineData("Remind take a break in 10 minutes")]
  [InlineData("REMIND me to call mom in 1 hour")]
  public void IsReminderCommandWithRemindPrefixReturnsTrue(string content)
  {
    var result = QuickCommandParser.IsReminderCommand(content);

    Assert.True(result);
  }

  [Theory]
  [InlineData("reminder take a break in 30 minutes")]
  [InlineData("REMINDER take a break in 30 minutes")]
  public void IsReminderCommandWithReminderPrefixReturnsTrue(string content)
  {
    var result = QuickCommandParser.IsReminderCommand(content);

    Assert.True(result);
  }

  [Theory]
  [InlineData("remember something")]
  [InlineData("hello world")]
  [InlineData("")]
  public void IsReminderCommandWithNonCommandReturnsFalse(string content)
  {
    var result = QuickCommandParser.IsReminderCommand(content);

    Assert.False(result);
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
