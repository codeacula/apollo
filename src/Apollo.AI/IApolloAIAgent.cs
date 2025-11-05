namespace Apollo.AI;

public interface IApolloAIAgent
{
  Task<string> ChatAsync(string username, string chatMessage);
}
