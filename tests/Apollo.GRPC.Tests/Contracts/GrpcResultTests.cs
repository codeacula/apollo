using Apollo.GRPC.Contracts;

using FluentResults;

namespace Apollo.GRPC.Tests.Contracts;

public class GrpcResultTests
{
  [Fact]
  public void ImplicitCastToFluentResultWithSuccessfulGrpcResultReturnsSuccessResult()
  {
    // Arrange
    const string testData = "Test data";


    // Act
    Result<string> result = (GrpcResult<string>)new()
    {
      IsSuccess = true,
      Data = testData,
      Errors = []
    };

    // Assert
    Assert.True(result.IsSuccess);
    Assert.False(result.IsFailed);
    Assert.Equal(testData, result.Value);
    Assert.Empty(result.Errors);
  }

  [Fact]
  public void ImplicitCastToFluentResultWithFailedGrpcResultReturnsFailedResult()
  {
    // Arrange
    const string errorMessage = "Something went wrong";
    const string errorCode = "ERR001";


    // Act
    Result<string> result = (GrpcResult<string>)new()
    {
      IsSuccess = false,
      Data = null,
      Errors =
      [
        new GrpcError(errorMessage, errorCode)
      ]
    };

    // Assert
    Assert.False(result.IsSuccess);
    Assert.True(result.IsFailed);
    _ = Assert.Single(result.Errors);
    Assert.Equal(errorMessage, result.Errors[0].Message);
  }

  [Fact]
  public void ImplicitCastToFluentResultWithMultipleErrorsReturnsResultWithAllErrors()
  {
    // Arrange


    // Act
    Result<string> result = (GrpcResult<string>)new()
    {
      IsSuccess = false,
      Data = null,
      Errors =
      [
        new GrpcError("Error 1", "ERR001"),
        new GrpcError("Error 2", "ERR002"),
        new GrpcError("Error 3", "ERR003")
      ]
    };

    // Assert
    Assert.False(result.IsSuccess);
    Assert.True(result.IsFailed);
    Assert.Equal(3, result.Errors.Count);
    Assert.Equal("Error 1", result.Errors[0].Message);
    Assert.Equal("Error 2", result.Errors[1].Message);
    Assert.Equal("Error 3", result.Errors[2].Message);
  }

  [Fact]
  public void ImplicitCastToFluentResultWithErrorCodePreservesErrorCode()
  {
    // Arrange
    const string errorCode = "CUSTOM_ERR";


    // Act
    Result<string> result = (GrpcResult<string>)new()
    {
      IsSuccess = false,
      Data = null,
      Errors =
      [
        new GrpcError("Error message", errorCode)
      ]
    };

    // Assert
    Assert.False(result.IsSuccess);
    _ = Assert.Single(result.Errors);
    Assert.True(result.Errors[0].HasMetadataKey("ErrorCode"));
    Assert.Equal(errorCode, result.Errors[0].Metadata["ErrorCode"]);
  }

  [Fact]
  public void ImplicitCastToFluentResultWithNullErrorCodePreservesEmptyString()
  {
    // Arrange


    // Act
    Result<string> result = (GrpcResult<string>)new()
    {
      IsSuccess = false,
      Data = null,
      Errors =
      [
        new GrpcError("Error message", null)
      ]
    };

    // Assert
    Assert.False(result.IsSuccess);
    _ = Assert.Single(result.Errors);
    Assert.True(result.Errors[0].HasMetadataKey("ErrorCode"));
    Assert.Equal(string.Empty, result.Errors[0].Metadata["ErrorCode"]);
  }

  [Fact]
  public void ImplicitCastToFluentResultWithComplexTypePreservesData()
  {
    // Arrange
    TestObject testObject = new("Test", 42);


    // Act
    Result<TestObject> result = (GrpcResult<TestObject>)new()
    {
      IsSuccess = true,
      Data = testObject,
      Errors = []
    };

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(testObject, result.Value);
    Assert.Equal("Test", result.Value.Name);
    Assert.Equal(42, result.Value.Value);
  }

  [Fact]
  public void ImplicitCastToFluentResultWithSuccessButNullDataReturnsFailedResult()
  {
    // Arrange


    // Act
    Result<string> result = (GrpcResult<string>)new()
    {
      IsSuccess = true,
      Data = null,
      Errors = []
    };

    // Assert
    Assert.False(result.IsSuccess);
    Assert.True(result.IsFailed);
    _ = Assert.Single(result.Errors);
    Assert.Contains("null data", result.Errors[0].Message, StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public void ImplicitCastToFluentResultWithFailedButNoErrorsReturnsFailedResultWithMessage()
  {
    // Arrange


    // Act
    Result<string> result = (GrpcResult<string>)new()
    {
      IsSuccess = false,
      Data = null,
      Errors = []
    };

    // Assert
    Assert.False(result.IsSuccess);
    Assert.True(result.IsFailed);
    _ = Assert.Single(result.Errors);
    Assert.Contains("no error information", result.Errors[0].Message, StringComparison.OrdinalIgnoreCase);
  }

  private sealed record TestObject(string Name, int Value);
}
