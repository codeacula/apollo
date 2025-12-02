using FluentResults;

namespace Apollo.Core.Infrastructure.API;

public interface IApolloAPIClient
{
  Task<Result<string>> SendMessageAsync(string message);
}
