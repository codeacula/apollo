using Apollo.AI.Helpers;

namespace Apollo.AI.Tests;

public class PromptyLoaderTests
{
  [Fact]
  public void LoadSystemPromptFromFileLoadsCorrectly()
  {
    // Arrange
    var promptyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Prompts", "Apollo.prompty");

    // Act
    var systemPrompt = PromptyLoader.LoadSystemPromptFromFile(promptyPath);

    // Assert
    Assert.NotNull(systemPrompt);
    Assert.NotEmpty(systemPrompt);
    Assert.Contains("Apollo", systemPrompt);
    Assert.Contains("friendly", systemPrompt);
    Assert.Contains("neurodivergent", systemPrompt);

    // Should not contain YAML frontmatter
    Assert.DoesNotContain("---", systemPrompt);
    Assert.DoesNotContain("name:", systemPrompt);
    Assert.DoesNotContain("model:", systemPrompt);

    Console.WriteLine($"Loaded prompt length: {systemPrompt.Length} characters");
    Console.WriteLine($"First 200 chars: {systemPrompt[..Math.Min(200, systemPrompt.Length)]}");
  }
}
