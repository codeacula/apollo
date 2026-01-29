using Apollo.Discord.Components;

using NetCord.Rest;

namespace Apollo.Discord.Tests.Components;

public class DailyAlertTimeConfigModalTests
{
  [Fact]
  public void ConstructorCreatesModal()
  {
    // Act
    DailyAlertTimeConfigModal modal = [];

    // Assert - Just verify the modal is created successfully
    Assert.NotNull(modal);
  }

  [Fact]
  public void ConstructorSetsTitleCorrectly()
  {
    // Act
    DailyAlertTimeConfigModal modal = [];

    // Assert
    Assert.Equal("Configure Daily Update", modal.Title);
  }

  [Fact]
  public void ConstructorHasTwoComponents()
  {
    // Act
    DailyAlertTimeConfigModal modal = [];

    // Assert
    Assert.Equal(2, modal.Components.Count());
  }

  [Fact]
  public void ConstructorHasTimeInputWithCorrectDefaults()
  {
    // Act
    DailyAlertTimeConfigModal modal = [];
    var components = modal.Components.ToList();

    // Assert - First component should be the time input wrapped in a label
    LabelProperties? timeLabel = components[0] as LabelProperties;
    Assert.NotNull(timeLabel);
    Assert.Equal("Time (HH:mm format, 24-hour)", timeLabel.Label);

    TextInputProperties? timeInput = timeLabel.Component as TextInputProperties;
    Assert.NotNull(timeInput);
    Assert.Equal(DailyAlertTimeConfigModal.TimeInputCustomId, timeInput.CustomId);
    Assert.Equal(TextInputStyle.Short, timeInput.Style);
    Assert.Equal(DailyAlertTimeConfigModal.DefaultTime, timeInput.Placeholder);
    Assert.Equal(DailyAlertTimeConfigModal.DefaultTime, timeInput.Value);
    Assert.False(timeInput.Required);
    Assert.Equal(5, timeInput.MinLength);
    Assert.Equal(5, timeInput.MaxLength);
  }

  [Fact]
  public void ConstructorHasMessageInputWithCorrectDefaults()
  {
    // Act
    DailyAlertTimeConfigModal modal = [];
    var components = modal.Components.ToList();

    // Assert - Second component should be the message input wrapped in a label
    LabelProperties? messageLabel = components[1] as LabelProperties;
    Assert.NotNull(messageLabel);
    Assert.Equal("Initial Message Text", messageLabel.Label);

    TextInputProperties? messageInput = messageLabel.Component as TextInputProperties;
    Assert.NotNull(messageInput);
    Assert.Equal(DailyAlertTimeConfigModal.MessageInputCustomId, messageInput.CustomId);
    Assert.Equal(TextInputStyle.Paragraph, messageInput.Style);
    Assert.Equal(DailyAlertTimeConfigModal.DefaultMessage, messageInput.Placeholder);
    Assert.Equal(DailyAlertTimeConfigModal.DefaultMessage, messageInput.Value);
    Assert.False(messageInput.Required);
    Assert.Equal(1, messageInput.MinLength);
    Assert.Equal(2000, messageInput.MaxLength);
  }

  [Fact]
  public void CustomIdConstantsAreCorrect()
  {
    // Assert
    Assert.Equal("daily_alert_time_config_modal", DailyAlertTimeConfigModal.CustomId);
    Assert.Equal("daily_alert_time_input", DailyAlertTimeConfigModal.TimeInputCustomId);
    Assert.Equal("daily_alert_message_input", DailyAlertTimeConfigModal.MessageInputCustomId);
  }

  [Fact]
  public void DefaultConstantsAreCorrect()
  {
    // Assert
    Assert.Equal("06:00", DailyAlertTimeConfigModal.DefaultTime);
    Assert.Equal("Good morning! What are your goals for today?", DailyAlertTimeConfigModal.DefaultMessage);
  }
}
