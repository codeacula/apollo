namespace Apollo.GRPC.Contracts;

[ProtoContract]
public sealed record GrpcResult<T> where T : class
{
  [ProtoMember(1)]
  public bool IsSuccess { get; init; }

  [ProtoMember(2)]
  public T? Data { get; init; }

  [ProtoMember(3)]
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
}

public static class GrpcResult
{
  public static GrpcResult<T> Ok<T>(T data) where T : class
  {
    return new()
    {
      IsSuccess = true,
      Data = data,
      Errors = []
    };
  }

  public static GrpcResult<T> Fail<T>(string error, string? errorCode = null) where T : class
  {
    return new()
    {
      IsSuccess = false,
      Data = null,
      Errors = [new GrpcError(error, errorCode)]
    };
  }

  public static GrpcResult<T> Fail<T>(GrpcError error) where T : class
  {
    return new()
    {
      IsSuccess = false,
      Data = null,
      Errors = [error]
    };
  }

  public static GrpcResult<T> Fail<T>(params GrpcError[] errors) where T : class
  {
    return new()
    {
      IsSuccess = false,
      Data = null,
      Errors = [.. errors]
    };
  }

  public static GrpcResult<T> Fail<T>(IEnumerable<GrpcError> errors) where T : class
  {
    return new()
    {
      IsSuccess = false,
      Data = null,
      Errors = [.. errors]
    };
  }
}
