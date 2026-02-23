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
    return grpcResult switch
    {
      { IsSuccess: true, Data: not null, Data: var data } => Result.Ok(data),
      { IsSuccess: true, Data: null } => Result.Fail<T>("GrpcResult marked as successful but contains null data"),
      { IsSuccess: false, Errors: var errors } when errors.Count > 0 => Result.Fail<T>(errors
        .Select(e => new Error(e.Message).WithMetadata("ErrorCode", e.ErrorCode ?? string.Empty))),
      { IsSuccess: false } => Result.Fail<T>("GrpcResult marked as failed but contains no error information"),
      _ => Result.Fail<T>("Unknown GrpcResult state")
    };
  }
}
