using Apollo.Discord.Components;
using NetCord.Rest;
using NetCord;

namespace Apollo.Discord.Tests.Components;

public class DailyAlertTimeConfigComponentTests
{
    [Fact]
    public void Constructor_SetsAccentColorToApolloGreen()
    {
        // Act
        var component = new DailyAlertTimeConfigComponent();

        // Assert
        Assert.Equal(Constants.Colors.ApolloGreen, component.AccentColor);
    }

    [Fact]
    public void Constructor_HasThreeComponents()
    {
        // Act
        var component = new DailyAlertTimeConfigComponent();

        // Assert
        Assert.Equal(3, component.Components.Count());
    }

    [Fact]
    public void Constructor_HasHeadingAndDescription()
    {
        // Act
        var component = new DailyAlertTimeConfigComponent();
        var components = component.Components.ToList();

        // Assert - First two components should be TextDisplayProperties
        Assert.IsType<TextDisplayProperties>(components[0]);
        Assert.IsType<TextDisplayProperties>(components[1]);
    }

    [Fact]
    public void Constructor_HasActionRowWithButton()
    {
        // Act
        var component = new DailyAlertTimeConfigComponent();
        var components = component.Components.ToList();

        // Assert
        var actionRow = components[2] as ActionRowProperties;
        Assert.NotNull(actionRow);
        Assert.Single(actionRow.Components);
    }

    [Fact]
    public void Constructor_ButtonHasCorrectProperties()
    {
        // Act
        var component = new DailyAlertTimeConfigComponent();
        var components = component.Components.ToList();
        var actionRow = components[2] as ActionRowProperties;
        var button = actionRow!.Components.First() as ButtonProperties;

        // Assert
        Assert.NotNull(button);
        Assert.Equal(DailyAlertTimeConfigComponent.ButtonCustomId, button.CustomId);
        Assert.Equal("Configure Time and Message", button.Label);
        Assert.Equal(ButtonStyle.Primary, button.Style);
    }

    [Fact]
    public void ButtonCustomId_IsCorrect()
    {
        // Assert
        Assert.Equal("daily_alert_time_config_button", DailyAlertTimeConfigComponent.ButtonCustomId);
    }
}
