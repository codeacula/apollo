namespace Apollo.Core.Infrastructure.API;

public sealed record ApiResponse<T>
{
  public T? Data { get; set; }
  public APIError? Error { get; set; }
  public bool IsSuccess => Error == null;

  public ApiResponse(T data)
  {
    Data = data;
    Error = null;
  }

  public ApiResponse(APIError error)
  {
    Data = default;
    Error = error;
  }
}
