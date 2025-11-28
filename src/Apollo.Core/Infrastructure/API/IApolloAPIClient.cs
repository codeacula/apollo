namespace Apollo.Core.Infrastructure.API;

public interface IApolloAPIClient
{
  Task<ApiResponse<string>> SendMessageAsync(string message);
}
