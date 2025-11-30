using System.Runtime.Serialization;

namespace Apollo.GRPC.Contracts;

[DataContract]
public sealed record GrpcError
{
  [DataMember(Order = 1)]
  public string Message { get; init; } = string.Empty;

  [DataMember(Order = 2)]
  public string? ErrorCode { get; init; }

  public GrpcError() { }

  public GrpcError(string message, string? errorCode = null)
  {
    Message = message;
    ErrorCode = errorCode;
  }
}
