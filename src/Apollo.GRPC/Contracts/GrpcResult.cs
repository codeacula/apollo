using System.Runtime.Serialization;

namespace Apollo.GRPC.Actions;

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
}
