namespace Apollo.GRPC;

public sealed record GrpcHostConfig
{
  public required string ApiToken { get; init; }
  public required string Host { get; init; }
  public required int Port { get; init; }
  public bool UseHttps { get; init; } = true;
  public bool ValidateSslCertificate { get; init; } = true;

  public string ApiHost => $"{(UseHttps ? "https" : "http")}://{Host}:{Port}";
}
