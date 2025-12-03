using System.Runtime.Serialization;

using FluentResults;

namespace Apollo.GRPC.Contracts;

[DataContract]
public sealed record GrpcResult<T> where T : class
{
  [DataMember(Order = 1)]
  public bool IsSuccess { get; init; }

  [DataMember(Order = 2)]
  public T? Data { get; init; }

  [DataMember(Order = 3)]
  public List<GrpcError> Errors { get; init; } = [];

  public static implicit operator GrpcResult<T>(T data)
  {
    return new()
    {
      IsSuccess = true,
      Data = data,
      Errors = []
    };
  }

  public static implicit operator GrpcResult<T>(GrpcError error)
  {
    return new()
    {
      IsSuccess = false,
      Data = null,
      Errors = [error]
    };
  }

  public static implicit operator GrpcResult<T>(GrpcError[] errors)
  {
    return new()
    {
      IsSuccess = false,
      Data = null,
      Errors = [.. errors]
    };
  }

  public static implicit operator Result<T>(GrpcResult<T> grpcResult)
  {
    if (grpcResult.IsSuccess)
    {
      if (grpcResult.Data is not null)
      {
        return Result.Ok(grpcResult.Data);
      }

      // Handle edge case: marked as success but data is null
      return Result.Fail<T>("GrpcResult marked as successful but contains null data");
    }

    // Handle failure case
    if (grpcResult.Errors.Count > 0)
    {
      var errors = grpcResult.Errors
        .Select(e => new Error(e.Message).WithMetadata("ErrorCode", e.ErrorCode ?? string.Empty));

      return Result.Fail<T>(errors);
    }

    // Handle edge case: marked as failed but no errors provided
    return Result.Fail<T>("GrpcResult marked as failed but contains no error information");
  }
}
