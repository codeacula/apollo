using Apollo.Core.Conversations;

using FluentResults;

namespace Apollo.Core.API;

public interface IApolloAPIClient
{
  Task<Result<string>> SendMessageAsync(NewMessage message);
}
