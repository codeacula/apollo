using Apollo.GRPC.Contracts;

using FluentResults;

namespace Apollo.GRPC.Tests.Contracts;

public class GrpcResultTests
{
  [Fact]
  public void ImplicitCastToFluentResult_WithSuccessfulGrpcResult_ReturnsSuccessResult()
  {
    // Arrange
    const string testData = "Test data";
    GrpcResult<string> grpcResult = new()
    {
      IsSuccess = true,
      Data = testData,
      Errors = []
    };

    // Act
    Result<string> result = grpcResult;

    // Assert
    Assert.True(result.IsSuccess);
    Assert.False(result.IsFailed);
    Assert.Equal(testData, result.Value);
    Assert.Empty(result.Errors);
  }

  [Fact]
  public void ImplicitCastToFluentResult_WithFailedGrpcResult_ReturnsFailedResult()
  {
    // Arrange
    const string errorMessage = "Something went wrong";
    const string errorCode = "ERR001";
    GrpcResult<string> grpcResult = new()
    {
      IsSuccess = false,
      Data = null,
      Errors =
      [
        new GrpcError(errorMessage, errorCode)
      ]
    };

    // Act
    Result<string> result = grpcResult;

    // Assert
    Assert.False(result.IsSuccess);
    Assert.True(result.IsFailed);
    Assert.Single(result.Errors);
    Assert.Equal(errorMessage, result.Errors[0].Message);
  }

  [Fact]
  public void ImplicitCastToFluentResult_WithMultipleErrors_ReturnsResultWithAllErrors()
  {
    // Arrange
    GrpcResult<string> grpcResult = new()
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

    // Act
    Result<string> result = grpcResult;

    // Assert
    Assert.False(result.IsSuccess);
    Assert.True(result.IsFailed);
    Assert.Equal(3, result.Errors.Count);
    Assert.Equal("Error 1", result.Errors[0].Message);
    Assert.Equal("Error 2", result.Errors[1].Message);
    Assert.Equal("Error 3", result.Errors[2].Message);
  }

  [Fact]
  public void ImplicitCastToFluentResult_WithErrorCode_PreservesErrorCode()
  {
    // Arrange
    const string errorCode = "CUSTOM_ERR";
    GrpcResult<string> grpcResult = new()
    {
      IsSuccess = false,
      Data = null,
      Errors =
      [
        new GrpcError("Error message", errorCode)
      ]
    };

    // Act
    Result<string> result = grpcResult;

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Single(result.Errors);
    Assert.True(result.Errors[0].HasMetadataKey("ErrorCode"));
    Assert.Equal(errorCode, result.Errors[0].Metadata["ErrorCode"]);
  }

  [Fact]
  public void ImplicitCastToFluentResult_WithNullErrorCode_PreservesEmptyString()
  {
    // Arrange
    GrpcResult<string> grpcResult = new()
    {
      IsSuccess = false,
      Data = null,
      Errors =
      [
        new GrpcError("Error message", null)
      ]
    };

    // Act
    Result<string> result = grpcResult;

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Single(result.Errors);
    Assert.True(result.Errors[0].HasMetadataKey("ErrorCode"));
    Assert.Equal(string.Empty, result.Errors[0].Metadata["ErrorCode"]);
  }

  [Fact]
  public void ImplicitCastToFluentResult_WithComplexType_PreservesData()
  {
    // Arrange
    TestObject testObject = new("Test", 42);
    GrpcResult<TestObject> grpcResult = new()
    {
      IsSuccess = true,
      Data = testObject,
      Errors = []
    };

    // Act
    Result<TestObject> result = grpcResult;

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(testObject, result.Value);
    Assert.Equal("Test", result.Value.Name);
    Assert.Equal(42, result.Value.Value);
  }

  [Fact]
  public void ImplicitCastToFluentResult_WithSuccessButNullData_ReturnsFailedResult()
  {
    // Arrange
    GrpcResult<string> grpcResult = new()
    {
      IsSuccess = true,
      Data = null,
      Errors = []
    };

    // Act
    Result<string> result = grpcResult;

    // Assert
    Assert.False(result.IsSuccess);
    Assert.True(result.IsFailed);
    Assert.Single(result.Errors);
    Assert.Contains("null data", result.Errors[0].Message, StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public void ImplicitCastToFluentResult_WithFailedButNoErrors_ReturnsFailedResultWithMessage()
  {
    // Arrange
    GrpcResult<string> grpcResult = new()
    {
      IsSuccess = false,
      Data = null,
      Errors = []
    };

    // Act
    Result<string> result = grpcResult;

    // Assert
    Assert.False(result.IsSuccess);
    Assert.True(result.IsFailed);
    Assert.Single(result.Errors);
    Assert.Contains("no error information", result.Errors[0].Message, StringComparison.OrdinalIgnoreCase);
  }

  private sealed record TestObject(string Name, int Value);
}
