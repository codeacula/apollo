using Apollo.Core.Infrastructure.API;

namespace Apollo.Core.Infrastructure;

public interface IApolloAPIClient
{
  Task<ApiResponse<string>> SendMessageAsync(string message);
}
