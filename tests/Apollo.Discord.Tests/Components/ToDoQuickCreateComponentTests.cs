using Apollo.Discord.Components;
using Apollo.Domain.Common.Enums;
using Apollo.Domain.Common.ValueObjects;
using Apollo.Domain.People.ValueObjects;
using Apollo.Domain.ToDos.Models;
using Apollo.Domain.ToDos.ValueObjects;

using NetCord.Rest;

namespace Apollo.Discord.Tests.Components;

public class ToDoQuickCreateComponentTests
{
  [Fact]
  public void EnergySelectCustomIdIncludesToDoId()
  {
    // Arrange
    var toDoId = new ToDoId(Guid.Parse("550e8400-e29b-41d4-a716-446655440000"));
    var todo = new ToDo
    {
      Id = toDoId,
      PersonId = new PersonId(Guid.NewGuid()),
      Description = new Description("Test task"),
      Priority = new Priority(Level.Blue),
      Energy = new Energy(Level.Blue),
      Interest = new Interest(Level.Blue),
      CreatedOn = new CreatedOn(DateTime.UtcNow),
      UpdatedOn = new UpdatedOn(DateTime.UtcNow)
    };

    // Act
    var component = new ToDoQuickCreateComponent(todo, null);
    var components = component.Components.ToList();

    // Find the StringMenuProperties for energy
    StringMenuProperties? energyMenu = components
      .OfType<StringMenuProperties>()
      .FirstOrDefault(m => m.CustomId.StartsWith("todo_energy_select"));

    // Assert
    Assert.NotNull(energyMenu);
    Assert.Equal($"todo_energy_select:{toDoId.Value}", energyMenu.CustomId);
  }

  [Fact]
  public void PrioritySelectCustomIdIncludesToDoId()
  {
    // Arrange
    var toDoId = new ToDoId(Guid.Parse("550e8400-e29b-41d4-a716-446655440000"));
    var todo = new ToDo
    {
      Id = toDoId,
      PersonId = new PersonId(Guid.NewGuid()),
      Description = new Description("Test task"),
      Priority = new Priority(Level.Blue),
      Energy = new Energy(Level.Blue),
      Interest = new Interest(Level.Blue),
      CreatedOn = new CreatedOn(DateTime.UtcNow),
      UpdatedOn = new UpdatedOn(DateTime.UtcNow)
    };

    // Act
    var component = new ToDoQuickCreateComponent(todo, null);
    var components = component.Components.ToList();

    // Find the StringMenuProperties for priority
    StringMenuProperties? priorityMenu = components
      .OfType<StringMenuProperties>()
      .FirstOrDefault(m => m.CustomId.StartsWith("todo_priority_select"));

    // Assert
    Assert.NotNull(priorityMenu);
    Assert.Equal($"todo_priority_select:{toDoId.Value}", priorityMenu.CustomId);
  }

  [Fact]
  public void InterestSelectCustomIdIncludesToDoId()
  {
    // Arrange
    var toDoId = new ToDoId(Guid.Parse("550e8400-e29b-41d4-a716-446655440000"));
    var todo = new ToDo
    {
      Id = toDoId,
      PersonId = new PersonId(Guid.NewGuid()),
      Description = new Description("Test task"),
      Priority = new Priority(Level.Blue),
      Energy = new Energy(Level.Blue),
      Interest = new Interest(Level.Blue),
      CreatedOn = new CreatedOn(DateTime.UtcNow),
      UpdatedOn = new UpdatedOn(DateTime.UtcNow)
    };

    // Act
    var component = new ToDoQuickCreateComponent(todo, null);
    var components = component.Components.ToList();

    // Find the StringMenuProperties for interest
    StringMenuProperties? interestMenu = components
      .OfType<StringMenuProperties>()
      .FirstOrDefault(m => m.CustomId.StartsWith("todo_interest_select"));

    // Assert
    Assert.NotNull(interestMenu);
    Assert.Equal($"todo_interest_select:{toDoId.Value}", interestMenu.CustomId);
  }

  [Fact]
  public void ReminderButtonCustomIdIncludesToDoId()
  {
    // Arrange
    var toDoId = new ToDoId(Guid.Parse("550e8400-e29b-41d4-a716-446655440000"));
    var todo = new ToDo
    {
      Id = toDoId,
      PersonId = new PersonId(Guid.NewGuid()),
      Description = new Description("Test task"),
      Priority = new Priority(Level.Blue),
      Energy = new Energy(Level.Blue),
      Interest = new Interest(Level.Blue),
      CreatedOn = new CreatedOn(DateTime.UtcNow),
      UpdatedOn = new UpdatedOn(DateTime.UtcNow)
    };

    // Act
    var component = new ToDoQuickCreateComponent(todo, null);
    var components = component.Components.ToList();

    // Find the ActionRowProperties with the button
    ActionRowProperties? actionRow = components.OfType<ActionRowProperties>().FirstOrDefault();
    Assert.NotNull(actionRow);

    ButtonProperties? button = actionRow.Components.First() as ButtonProperties;

    // Assert
    Assert.NotNull(button);
    Assert.Equal($"todo_reminder_button:{toDoId.Value}", button.CustomId);
  }

  [Fact]
  public void DefaultEnergySelectionIsGreen()
  {
    // Arrange
    var todo = new ToDo
    {
      Id = new ToDoId(Guid.NewGuid()),
      PersonId = new PersonId(Guid.NewGuid()),
      Description = new Description("Test task"),
      Priority = new Priority(Level.Blue),
      Energy = new Energy(Level.Blue),
      Interest = new Interest(Level.Blue),
      CreatedOn = new CreatedOn(DateTime.UtcNow),
      UpdatedOn = new UpdatedOn(DateTime.UtcNow)
    };

    // Act
    var component = new ToDoQuickCreateComponent(todo, null);
    var components = component.Components.ToList();

    // Find the energy menu
    StringMenuProperties? energyMenu = components
      .OfType<StringMenuProperties>()
      .FirstOrDefault(m => m.CustomId.StartsWith("todo_energy_select"));

    Assert.NotNull(energyMenu);

    // Get all the select menu options
    var selectMenuOptions = energyMenu.ToList();

    // Find which option has Default = true
    var defaultOption = selectMenuOptions.FirstOrDefault(opt => opt.Default);

    // Assert
    Assert.NotNull(defaultOption);
    Assert.Equal("green", defaultOption.Value);
  }
}
