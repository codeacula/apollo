namespace Apollo.GRPC.Contracts;

[ProtoContract]
public sealed record GrpcError
{
  [ProtoMember(1)]
  public string Message { get; init; } = string.Empty;

  [ProtoMember(2)]
  public string? ErrorCode { get; init; }

  public GrpcError() { }

  public GrpcError(string message, string? errorCode = null)
  {
    Message = message;
    ErrorCode = errorCode;
  }
}
