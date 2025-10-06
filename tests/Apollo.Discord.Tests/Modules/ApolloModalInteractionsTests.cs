using Apollo.Discord.Components;
using NetCord.Rest;

namespace Apollo.Discord.Tests.Modules;

public class ApolloModalInteractionsTests
{
    [Fact]
    public void DailyAlertTimeConfigModal_TimeInput_ShouldNotBeRequired()
    {
        // Arrange & Act
        var modal = new DailyAlertTimeConfigModal();
        var components = modal.Components.ToList();
        var timeLabel = components[0] as LabelProperties;
        var timeInput = timeLabel!.Component as TextInputProperties;

        // Assert - Time input should NOT be required to allow default values
        Assert.False(timeInput!.Required);
    }

    [Fact]
    public void DailyAlertTimeConfigModal_MessageInput_ShouldNotBeRequired()
    {
        // Arrange & Act
        var modal = new DailyAlertTimeConfigModal();
        var components = modal.Components.ToList();
        var messageLabel = components[1] as LabelProperties;
        var messageInput = messageLabel!.Component as TextInputProperties;

        // Assert - Message input should NOT be required to allow default values
        Assert.False(messageInput!.Required);
    }
}
