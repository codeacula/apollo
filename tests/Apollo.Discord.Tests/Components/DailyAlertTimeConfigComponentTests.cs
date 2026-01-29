using Apollo.Discord.Components;

using NetCord;
using NetCord.Rest;

namespace Apollo.Discord.Tests.Components;

public class DailyAlertTimeConfigComponentTests
{
  [Fact]
  public void ConstructorSetsAccentColorToApolloGreen()
  {
    // Act
    DailyAlertTimeConfigComponent component = [];

    // Assert
    Assert.Equal(Constants.Colors.ApolloGreen, component.AccentColor);
  }

  [Fact]
  public void ConstructorHasThreeComponents()
  {
    // Act
    DailyAlertTimeConfigComponent component = [];

    // Assert
    Assert.Equal(3, component.Components.Count());
  }

  [Fact]
  public void ConstructorHasHeadingAndDescription()
  {
    // Act
    DailyAlertTimeConfigComponent component = [];
    var components = component.Components.ToList();

    // Assert - First two components should be TextDisplayProperties
    _ = Assert.IsType<TextDisplayProperties>(components[0]);
    _ = Assert.IsType<TextDisplayProperties>(components[1]);
  }

  [Fact]
  public void ConstructorHasActionRowWithButton()
  {
    // Act
    DailyAlertTimeConfigComponent component = [];
    var components = component.Components.ToList();

    // Assert
    ActionRowProperties? actionRow = components[2] as ActionRowProperties;
    Assert.NotNull(actionRow);
    _ = Assert.Single(actionRow.Components);
  }

  [Fact]
  public void ConstructorButtonHasCorrectProperties()
  {
    // Act
    DailyAlertTimeConfigComponent component = [];
    var components = component.Components.ToList();
    ActionRowProperties? actionRow = components[2] as ActionRowProperties;
    ButtonProperties? button = actionRow!.Components.First() as ButtonProperties;

    // Assert
    Assert.NotNull(button);
    Assert.Equal(DailyAlertTimeConfigComponent.ButtonCustomId, button.CustomId);
    Assert.Equal("Configure Time and Message", button.Label);
    Assert.Equal(ButtonStyle.Primary, button.Style);
  }

  [Fact]
  public void ButtonCustomIdIsCorrect()
  {
    // Assert
    Assert.Equal("daily_alert_time_config_button", DailyAlertTimeConfigComponent.ButtonCustomId);
  }
}
