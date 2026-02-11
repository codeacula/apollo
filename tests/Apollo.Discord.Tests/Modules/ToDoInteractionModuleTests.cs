using Apollo.Discord.Modules;

namespace Apollo.Discord.Tests.Modules;

public sealed class ToDoInteractionModuleTests
{
  [Fact]
  public void ToDoPriorityInteractionModuleInitializes()
  {
    // Act
    var module = new ToDoPriorityInteractionModule();

    // Assert
    Assert.NotNull(module);
  }

  [Fact]
  public void ToDoPriorityInteractionModuleHasHandlePriorityMethod()
  {
    // Arrange
    _ = new ToDoPriorityInteractionModule();

    // Act
    var method = typeof(ToDoPriorityInteractionModule).GetMethod("HandlePriority");

    // Assert
    Assert.NotNull(method);
  }

  [Fact]
  public void ToDoEnergyInteractionModuleInitializes()
  {
    // Act
    var module = new ToDoEnergyInteractionModule();

    // Assert
    Assert.NotNull(module);
  }

  [Fact]
  public void ToDoEnergyInteractionModuleHasHandleEnergyMethod()
  {
    // Arrange
    _ = new ToDoEnergyInteractionModule();

    // Act
    var method = typeof(ToDoEnergyInteractionModule).GetMethod("HandleEnergy");

    // Assert
    Assert.NotNull(method);
  }

  [Fact]
  public void ToDoInterestInteractionModuleInitializes()
  {
    // Act
    var module = new ToDoInterestInteractionModule();

    // Assert
    Assert.NotNull(module);
  }

  [Fact]
  public void ToDoInterestInteractionModuleHasHandleInterestMethod()
  {
    // Arrange
    _ = new ToDoInterestInteractionModule();

    // Act
    var method = typeof(ToDoInterestInteractionModule).GetMethod("HandleInterest");

    // Assert
    Assert.NotNull(method);
  }

  [Fact]
  public void ToDoReminderInteractionModuleInitializes()
  {
    // Act
    var module = new ToDoReminderInteractionModule();

    // Assert
    Assert.NotNull(module);
  }

  [Fact]
  public void ToDoReminderInteractionModuleHasHandleReminderButtonMethod()
  {
    // Arrange
    _ = new ToDoReminderInteractionModule();

    // Act
    var method = typeof(ToDoReminderInteractionModule).GetMethod("HandleReminderButton");

    // Assert
    Assert.NotNull(method);
  }

  [Fact]
  public void ToDoEditInteractionModuleInitializes()
  {
    // Act
    var module = new ToDoEditInteractionModule();

    // Assert
    Assert.NotNull(module);
  }

  [Fact]
  public void ToDoEditInteractionModuleHasHandleEditButtonMethod()
  {
    // Arrange
    _ = new ToDoEditInteractionModule();

    // Act
    var method = typeof(ToDoEditInteractionModule).GetMethod("HandleEditButton");

    // Assert
    Assert.NotNull(method);
  }

  [Fact]
  public void ToDoDeleteInteractionModuleInitializes()
  {
    // Act
    var module = new ToDoDeleteInteractionModule();

    // Assert
    Assert.NotNull(module);
  }

  [Fact]
  public void ToDoDeleteInteractionModuleHasHandleDeleteButtonMethod()
  {
    // Arrange
    _ = new ToDoDeleteInteractionModule();

    // Act
    var method = typeof(ToDoDeleteInteractionModule).GetMethod("HandleDeleteButton");

    // Assert
    Assert.NotNull(method);
  }

  [Fact]
  public void ToDoPriorityInteractionModuleHandlePriorityReturnsString()
  {
    // Arrange
    _ = new ToDoPriorityInteractionModule();
    var method = typeof(ToDoPriorityInteractionModule).GetMethod("HandlePriority");

    // Act & Assert
    Assert.NotNull(method);
    Assert.Equal(typeof(string), method!.ReturnType);
  }

  [Fact]
  public void ToDoEnergyInteractionModuleHandleEnergyReturnsString()
  {
    // Arrange
    _ = new ToDoEnergyInteractionModule();
    var method = typeof(ToDoEnergyInteractionModule).GetMethod("HandleEnergy");

    // Act & Assert
    Assert.NotNull(method);
    Assert.Equal(typeof(string), method!.ReturnType);
  }

  [Fact]
  public void ToDoReminderInteractionModuleHandleReminderButtonReturnsString()
  {
    // Arrange
    _ = new ToDoReminderInteractionModule();
    var method = typeof(ToDoReminderInteractionModule).GetMethod("HandleReminderButton");

    // Act & Assert
    Assert.NotNull(method);
    Assert.Equal(typeof(string), method!.ReturnType);
  }

  [Fact]
  public void ToDoEditInteractionModuleHandleEditButtonReturnsString()
  {
    // Arrange
    _ = new ToDoEditInteractionModule();
    var method = typeof(ToDoEditInteractionModule).GetMethod("HandleEditButton");

    // Act & Assert
    Assert.NotNull(method);
    Assert.Equal(typeof(string), method!.ReturnType);
  }

  [Fact]
  public void ToDoDeleteInteractionModuleHandleDeleteButtonReturnsString()
  {
    // Arrange
    _ = new ToDoDeleteInteractionModule();
    var method = typeof(ToDoDeleteInteractionModule).GetMethod("HandleDeleteButton");

    // Act & Assert
    Assert.NotNull(method);
    Assert.Equal(typeof(string), method!.ReturnType);
  }
}
