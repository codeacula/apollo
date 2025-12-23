using Apollo.Core;
using FluentResults;

namespace Apollo.Core.Tests;

public class ResultExtensionsTests
{
  [Fact]
  public void GetErrorMessagesReturnsEmptyStringWhenNoErrors()
  {
    // Arrange
    var result = Result.Ok();

    // Act
    var errorMessages = result.GetErrorMessages();

    // Assert
    Assert.Equal(string.Empty, errorMessages);
  }

  [Fact]
  public void GetErrorMessagesReturnsSingleErrorMessage()
  {
    // Arrange
    var result = Result.Fail("Error message");

    // Act
    var errorMessages = result.GetErrorMessages();

    // Assert
    Assert.Equal("Error message", errorMessages);
  }

  [Fact]
  public void GetErrorMessagesReturnsMultipleErrorMessagesWithDefaultSeparator()
  {
    // Arrange
    var result = Result.Fail("Error 1")
      .WithError("Error 2")
      .WithError("Error 3");

    // Act
    var errorMessages = result.GetErrorMessages();

    // Assert
    Assert.Equal("Error 1, Error 2, Error 3", errorMessages);
  }

  [Fact]
  public void GetErrorMessagesReturnsMultipleErrorMessagesWithCustomSeparator()
  {
    // Arrange
    var result = Result.Fail("Error 1")
      .WithError("Error 2")
      .WithError("Error 3");

    // Act
    var errorMessages = result.GetErrorMessages("; ");

    // Assert
    Assert.Equal("Error 1; Error 2; Error 3", errorMessages);
  }

  [Fact]
  public void GetErrorMessagesReturnsMultipleErrorMessagesWithNewlineSeparator()
  {
    // Arrange
    var result = Result.Fail("Error 1")
      .WithError("Error 2")
      .WithError("Error 3");

    // Act
    var errorMessages = result.GetErrorMessages("\n");

    // Assert
    Assert.Equal("Error 1\nError 2\nError 3", errorMessages);
  }

  [Fact]
  public void GetErrorMessagesWorksWithTypedResult()
  {
    // Arrange
    var result = Result.Fail<int>("Error 1")
      .WithError("Error 2");

    // Act
    var errorMessages = result.GetErrorMessages();

    // Assert
    Assert.Equal("Error 1, Error 2", errorMessages);
  }

  [Fact]
  public void GetErrorMessagesWorksWithEmptyTypedResult()
  {
    // Arrange
    var result = Result.Ok<string>("Success");

    // Act
    var errorMessages = result.GetErrorMessages();

    // Assert
    Assert.Equal(string.Empty, errorMessages);
  }
}
