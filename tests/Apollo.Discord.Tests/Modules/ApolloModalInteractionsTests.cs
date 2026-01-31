using Apollo.Discord.Components;

using NetCord.Rest;

namespace Apollo.Discord.Tests.Modules;

public class ApolloModalInteractionsTests
{
  [Fact]
  public void DailyAlertTimeConfigModalTimeInputShouldNotBeRequired()
  {
    // Arrange & Act
    DailyAlertTimeConfigModal modal = [];
    var components = modal.Components.ToList();
    LabelProperties? timeLabel = components[0] as LabelProperties;
    TextInputProperties? timeInput = timeLabel!.Component as TextInputProperties;

    // Assert - Time input should NOT be required to allow default values
    Assert.False(timeInput!.Required);
  }

  [Fact]
  public void DailyAlertTimeConfigModalMessageInputShouldNotBeRequired()
  {
    // Arrange & Act
    DailyAlertTimeConfigModal modal = [];
    var components = modal.Components.ToList();
    LabelProperties? messageLabel = components[1] as LabelProperties;
    TextInputProperties? messageInput = messageLabel!.Component as TextInputProperties;

    // Assert - Message input should NOT be required to allow default values
    Assert.False(messageInput!.Required);
  }
}
