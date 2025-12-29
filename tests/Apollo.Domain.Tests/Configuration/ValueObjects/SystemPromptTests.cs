using Apollo.Domain.Configuration.ValueObjects;

namespace Apollo.Domain.Tests.Configuration.ValueObjects;

public class SystemPromptTests
{
  [Fact]
  public void SystemPromptStoresValue()
  {
    // Arrange & Act
    var prompt = new SystemPrompt("You are a helpful assistant.");

    // Assert
    Assert.Equal("You are a helpful assistant.", prompt.Value);
  }

  [Fact]
  public void SystemPromptEqualityWorksCorrectly()
  {
    // Arrange
    var prompt1 = new SystemPrompt("You are helpful.");
    var prompt2 = new SystemPrompt("You are helpful.");
    var prompt3 = new SystemPrompt("You are different.");

    // Act & Assert
    Assert.Equal(prompt1, prompt2);
    Assert.NotEqual(prompt1, prompt3);
  }
}
