using Apollo.Discord.Components;
using NetCord;
using NetCord.Rest;

namespace Apollo.Discord.Tests.Components;

public class DailyAlertSetupComponentTests
{
    [Fact]
    public void Constructor_WithNoParameters_CreatesComponent()
    {
        var component = new DailyAlertSetupComponent();

        Assert.NotNull(component);
        Assert.Equal(Constants.Colors.ApolloGreen, component.AccentColor);
    }

    [Fact]
    public void Constructor_WithAllParameters_PreSelectsValues()
    {
        var channelId = 123456789UL;
        var roleId = 987654321UL;
        var time = "08:00";
        var message = "Good morning!";

        var component = new DailyAlertSetupComponent(channelId, roleId, time, message);

        Assert.NotNull(component);
    }

    [Fact]
    public void Constructor_HasRequiredComponents()
    {
        var component = new DailyAlertSetupComponent();
        var components = component.Components.ToList();

        Assert.True(components.Count >= 5);
    }

    [Fact]
    public void Constructor_HasChannelMenu()
    {
        var component = new DailyAlertSetupComponent();
        var components = component.Components.ToList();

        var channelMenu = components.OfType<ChannelMenuProperties>().FirstOrDefault();
        Assert.NotNull(channelMenu);
        Assert.Equal(DailyAlertSetupComponent.ChannelSelectCustomId, channelMenu.CustomId);
        Assert.Contains(ChannelType.ForumGuildChannel, channelMenu.ChannelTypes!);
    }

    [Fact]
    public void Constructor_HasRoleMenu()
    {
        var component = new DailyAlertSetupComponent();
        var components = component.Components.ToList();

        var roleMenu = components.OfType<RoleMenuProperties>().FirstOrDefault();
        Assert.NotNull(roleMenu);
        Assert.Equal(DailyAlertSetupComponent.RoleSelectCustomId, roleMenu.CustomId);
    }

    [Fact]
    public void Constructor_IncompleteConfig_ShowsOnlyConfigureButton()
    {
        var component = new DailyAlertSetupComponent();
        var components = component.Components.ToList();

        var actionRow = components.OfType<ActionRowProperties>().FirstOrDefault();
        Assert.NotNull(actionRow);
        Assert.Single(actionRow.Components);

        var button = actionRow.Components.First() as ButtonProperties;
        Assert.NotNull(button);
        Assert.Equal(DailyAlertSetupComponent.ConfigureTimeButtonCustomId, button.CustomId);
    }

    [Fact]
    public void Constructor_CompleteConfig_ShowsBothButtons()
    {
        var component = new DailyAlertSetupComponent(123UL, 456UL, "08:00", "Test message");
        var components = component.Components.ToList();

        var actionRow = components.OfType<ActionRowProperties>().FirstOrDefault();
        Assert.NotNull(actionRow);
        Assert.Equal(2, actionRow.Components.Count());

        var configureButton = actionRow.Components.First() as ButtonProperties;
        Assert.NotNull(configureButton);
        Assert.Equal(DailyAlertSetupComponent.ConfigureTimeButtonCustomId, configureButton.CustomId);

        var saveButton = actionRow.Components.Last() as ButtonProperties;
        Assert.NotNull(saveButton);
        Assert.Equal(DailyAlertSetupComponent.SaveButtonCustomId, saveButton.CustomId);
        Assert.Equal(ButtonStyle.Success, saveButton.Style);
    }

    [Fact]
    public void CustomIdConstants_AreCorrect()
    {
        Assert.Equal("daily_alert_setup_channel", DailyAlertSetupComponent.ChannelSelectCustomId);
        Assert.Equal("daily_alert_setup_role", DailyAlertSetupComponent.RoleSelectCustomId);
        Assert.Equal("daily_alert_setup_time_button", DailyAlertSetupComponent.ConfigureTimeButtonCustomId);
        Assert.Equal("daily_alert_setup_save_button", DailyAlertSetupComponent.SaveButtonCustomId);
    }
}
