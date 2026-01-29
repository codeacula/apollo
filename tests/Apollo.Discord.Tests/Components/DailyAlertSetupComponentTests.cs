using Apollo.Discord.Components;

using NetCord;
using NetCord.Rest;

namespace Apollo.Discord.Tests.Components;

public class DailyAlertSetupComponentTests
{
  [Fact]
  public void ConstructorWithNoParametersCreatesComponent()
  {
    DailyAlertSetupComponent component = [];

    Assert.NotNull(component);
    Assert.Equal(Constants.Colors.ApolloGreen, component.AccentColor);
  }

  [Fact]
  public void ConstructorWithAllParametersPreSelectsValues()
  {
    const ulong channelId = 123456789UL;
    const ulong roleId = 987654321UL;
    const string time = "08:00";
    const string message = "Good morning!";

    DailyAlertSetupComponent component = new(channelId, roleId, time, message);

    Assert.NotNull(component);
  }

  [Fact]
  public void ConstructorHasRequiredComponents()
  {
    DailyAlertSetupComponent component = [];
    var components = component.Components.ToList();

    Assert.True(components.Count >= 5);
  }

  [Fact]
  public void ConstructorHasChannelMenu()
  {
    DailyAlertSetupComponent component = [];
    var components = component.Components.ToList();

    ChannelMenuProperties? channelMenu = components.OfType<ChannelMenuProperties>().FirstOrDefault();
    Assert.NotNull(channelMenu);
    Assert.Equal(DailyAlertSetupComponent.ChannelSelectCustomId, channelMenu.CustomId);
    Assert.Contains(ChannelType.ForumGuildChannel, channelMenu.ChannelTypes!);
  }

  [Fact]
  public void ConstructorHasRoleMenu()
  {
    DailyAlertSetupComponent component = [];
    var components = component.Components.ToList();

    RoleMenuProperties? roleMenu = components.OfType<RoleMenuProperties>().FirstOrDefault();
    Assert.NotNull(roleMenu);
    Assert.Equal(DailyAlertSetupComponent.RoleSelectCustomId, roleMenu.CustomId);
  }

  [Fact]
  public void ConstructorIncompleteConfigShowsOnlyConfigureButton()
  {
    DailyAlertSetupComponent component = [];
    var components = component.Components.ToList();

    ActionRowProperties? actionRow = components.OfType<ActionRowProperties>().FirstOrDefault();
    Assert.NotNull(actionRow);
    _ = Assert.Single(actionRow.Components);

    ButtonProperties? button = actionRow.Components.First() as ButtonProperties;
    Assert.NotNull(button);
    Assert.Equal(DailyAlertSetupComponent.ConfigureTimeButtonCustomId, button.CustomId);
  }

  [Fact]
  public void ConstructorCompleteConfigShowsBothButtons()
  {
    DailyAlertSetupComponent component = new(123UL, 456UL, "08:00", "Test message");
    var components = component.Components.ToList();

    ActionRowProperties? actionRow = components.OfType<ActionRowProperties>().FirstOrDefault();
    Assert.NotNull(actionRow);
    Assert.Equal(2, actionRow.Components.Count());

    ButtonProperties? configureButton = actionRow.Components.First() as ButtonProperties;
    Assert.NotNull(configureButton);
    Assert.Equal(DailyAlertSetupComponent.ConfigureTimeButtonCustomId, configureButton.CustomId);

    ButtonProperties? saveButton = actionRow.Components.Last() as ButtonProperties;
    Assert.NotNull(saveButton);
    Assert.Equal(DailyAlertSetupComponent.SaveButtonCustomId, saveButton.CustomId);
    Assert.Equal(ButtonStyle.Success, saveButton.Style);
  }

  [Fact]
  public void CustomIdConstantsAreCorrect()
  {
    Assert.Equal("daily_alert_setup_channel", DailyAlertSetupComponent.ChannelSelectCustomId);
    Assert.Equal("daily_alert_setup_role", DailyAlertSetupComponent.RoleSelectCustomId);
    Assert.Equal("daily_alert_setup_time_button", DailyAlertSetupComponent.ConfigureTimeButtonCustomId);
    Assert.Equal("daily_alert_setup_save_button", DailyAlertSetupComponent.SaveButtonCustomId);
  }
}
