using Apollo.AI.Tooling;

namespace Apollo.AI.Tests.Tooling;

public class ToolCallMatchersTests
{
  [Fact]
  public void IsCreateToDoReturnsTrueForMatchingPluginAndFunction()
  {
    var result = ToolCallMatchers.IsCreateToDo("ToDos", "create_todo");

    Assert.True(result);
  }

  [Fact]
  public void IsCreateToDoIsCaseInsensitive()
  {
    var result = ToolCallMatchers.IsCreateToDo("TODOS", "CREATE_TODO");

    Assert.True(result);
  }

  [Fact]
  public void IsCreateToDoReturnsFalseForWrongPlugin()
  {
    var result = ToolCallMatchers.IsCreateToDo("Person", "create_todo");

    Assert.False(result);
  }

  [Fact]
  public void IsCreateToDoReturnsFalseForWrongFunction()
  {
    var result = ToolCallMatchers.IsCreateToDo("ToDos", "delete_todo");

    Assert.False(result);
  }

  [Fact]
  public void IsCreateToDoReturnsFalseForNullValues()
  {
    Assert.False(ToolCallMatchers.IsCreateToDo(null, "create_todo"));
    Assert.False(ToolCallMatchers.IsCreateToDo("ToDos", null));
    Assert.False(ToolCallMatchers.IsCreateToDo(null, null));
  }

  [Fact]
  public void IsSetTimezoneReturnsTrueForMatchingPluginAndFunction()
  {
    var result = ToolCallMatchers.IsSetTimezone("Person", "set_timezone");

    Assert.True(result);
  }

  [Fact]
  public void IsSetTimezoneIsCaseInsensitive()
  {
    var result = ToolCallMatchers.IsSetTimezone("PERSON", "SET_TIMEZONE");

    Assert.True(result);
  }

  [Fact]
  public void IsSetTimezoneReturnsFalseForWrongPlugin()
  {
    var result = ToolCallMatchers.IsSetTimezone("ToDos", "set_timezone");

    Assert.False(result);
  }

  [Fact]
  public void IsSetTimezoneReturnsFalseForWrongFunction()
  {
    var result = ToolCallMatchers.IsSetTimezone("Person", "get_timezone");

    Assert.False(result);
  }

  [Fact]
  public void IsSetTimezoneReturnsFalseForNullValues()
  {
    Assert.False(ToolCallMatchers.IsSetTimezone(null, "set_timezone"));
    Assert.False(ToolCallMatchers.IsSetTimezone("Person", null));
    Assert.False(ToolCallMatchers.IsSetTimezone(null, null));
  }

  [Fact]
  public void IsBlockedAfterCreateToDoReturnsTrueForCompleteTodo()
  {
    var result = ToolCallMatchers.IsBlockedAfterCreateToDo("ToDos", "complete_todo");

    Assert.True(result);
  }

  [Fact]
  public void IsBlockedAfterCreateToDoReturnsTrueForDeleteTodo()
  {
    var result = ToolCallMatchers.IsBlockedAfterCreateToDo("ToDos", "delete_todo");

    Assert.True(result);
  }

  [Fact]
  public void IsBlockedAfterCreateToDoIsCaseInsensitive()
  {
    var result = ToolCallMatchers.IsBlockedAfterCreateToDo("TODOS", "COMPLETE_TODO");

    Assert.True(result);
  }

  [Fact]
  public void IsBlockedAfterCreateToDoReturnsFalseForWrongPlugin()
  {
    var result = ToolCallMatchers.IsBlockedAfterCreateToDo("Person", "complete_todo");

    Assert.False(result);
  }

  [Fact]
  public void IsBlockedAfterCreateToDoReturnsFalseForAllowedFunction()
  {
    var result = ToolCallMatchers.IsBlockedAfterCreateToDo("ToDos", "update_todo");

    Assert.False(result);
  }

  [Fact]
  public void IsBlockedAfterCreateToDoReturnsFalseForNullValues()
  {
    Assert.False(ToolCallMatchers.IsBlockedAfterCreateToDo(null, "complete_todo"));
    Assert.False(ToolCallMatchers.IsBlockedAfterCreateToDo("ToDos", null));
    Assert.False(ToolCallMatchers.IsBlockedAfterCreateToDo(null, null));
  }

  [Fact]
  public void IsBlockedAfterCreateReminderReturnsTrueForDeleteReminder()
  {
    var result = ToolCallMatchers.IsBlockedAfterCreateReminder("Reminders", "delete_reminder");

    Assert.True(result);
  }

  [Fact]
  public void IsBlockedAfterCreateReminderReturnsTrueForUnlinkReminder()
  {
    var result = ToolCallMatchers.IsBlockedAfterCreateReminder("Reminders", "unlink_reminder");

    Assert.True(result);
  }

  [Fact]
  public void IsBlockedAfterCreateReminderIsCaseInsensitive()
  {
    var result = ToolCallMatchers.IsBlockedAfterCreateReminder("REMINDERS", "DELETE_REMINDER");

    Assert.True(result);
  }

  [Fact]
  public void IsBlockedAfterCreateReminderReturnsFalseForWrongPlugin()
  {
    var result = ToolCallMatchers.IsBlockedAfterCreateReminder("ToDos", "delete_reminder");

    Assert.False(result);
  }

  [Fact]
  public void IsBlockedAfterCreateReminderReturnsFalseForAllowedFunction()
  {
    var result = ToolCallMatchers.IsBlockedAfterCreateReminder("Reminders", "update_reminder");

    Assert.False(result);
  }

  [Fact]
  public void IsBlockedAfterCreateReminderReturnsFalseForNullValues()
  {
    Assert.False(ToolCallMatchers.IsBlockedAfterCreateReminder(null, "delete_reminder"));
    Assert.False(ToolCallMatchers.IsBlockedAfterCreateReminder("Reminders", null));
    Assert.False(ToolCallMatchers.IsBlockedAfterCreateReminder(null, null));
  }

  [Fact]
  public void ConstantsHaveExpectedValues()
  {
    Assert.Equal("ToDos", ToolCallMatchers.ToDoPluginName);
    Assert.Equal("Person", ToolCallMatchers.PersonPluginName);
    Assert.Equal("Reminders", ToolCallMatchers.RemindersPluginName);
    Assert.Equal("create_todo", ToolCallMatchers.CreateToDoFunction);
    Assert.Equal("set_timezone", ToolCallMatchers.SetTimezoneFunction);
    Assert.Equal(5, ToolCallMatchers.MaxToolCalls);
    Assert.Equal(3, ToolCallMatchers.MaxConsecutiveRepeats);
  }
}
