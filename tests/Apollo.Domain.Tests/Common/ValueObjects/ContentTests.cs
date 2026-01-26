using Apollo.Domain.Common.ValueObjects;

namespace Apollo.Domain.Tests.Common.ValueObjects;

public class ContentTests
{
  [Fact]
  public void ContentStoresValue()
  {
    // Arrange & Act
    var content = new Content("Test content");

    // Assert
    Assert.Equal("Test content", content.Value);
  }

  [Fact]
  public void ContentEqualityWorksCorrectly()
  {
    // Arrange
    var content1 = new Content("Test");
    var content2 = new Content("Test");
    var content3 = new Content("Other");

    // Act & Assert
    Assert.Equal(content1, content2);
    Assert.NotEqual(content1, content3);
  }
}
