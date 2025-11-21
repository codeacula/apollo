namespace Apollo.Core.Infrastructure;

public interface IApolloAPIClient
{
  Task<string> SendMessageAsync(string message);
}
