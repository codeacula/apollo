using Apollo.Core.API;

namespace Apollo.Core.Tests.API;

public class ApiResponseTests
{
  [Fact]
  public void ApiResponseWithDataIsSuccess()
  {
    // Arrange & Act
    var response = new ApiResponse<string>("test data");

    // Assert
    Assert.True(response.IsSuccess);
    Assert.Equal("test data", response.Data);
    Assert.Null(response.Error);
  }

  [Fact]
  public void ApiResponseWithErrorIsNotSuccess()
  {
    // Arrange
    var error = new APIError("ERR001", "Test error");

    // Act
    var response = new ApiResponse<string>(error);

    // Assert
    Assert.False(response.IsSuccess);
    Assert.Null(response.Data);
    Assert.Equal(error, response.Error);
  }

  [Fact]
  public void ApiResponseErrorPropertiesAreAccessible()
  {
    // Arrange
    var error = new APIError("ERR001", "Test error");
    var response = new ApiResponse<string>(error);

    // Act & Assert
    Assert.Equal("ERR001", response.Error?.Code);
    Assert.Equal("Test error", response.Error?.Message);
  }
}
