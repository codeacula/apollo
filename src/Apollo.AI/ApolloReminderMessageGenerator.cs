using Apollo.AI.DTOs;
using Apollo.Core.ToDos;

using FluentResults;

namespace Apollo.AI;

public sealed class ApolloReminderMessageGenerator(IApolloAIAgent apolloAIAgent) : IReminderMessageGenerator
{
  private const string ReminderSystemPrompt = """
    You are Apollo, a friendly and supportive task assistant who is also a large orange & white cat.
    Your job is to create a brief, encouraging reminder message for a user about their pending tasks.
    
    Guidelines:
    - Be casual, warm, and supportive
    - Keep the message concise (2-3 sentences max before the task list)
    - Include a cat-themed touch when appropriate (purrs, meows, gentle nudges)
    - Focus on being encouraging, not nagging
    - Format the tasks as a bulleted list
    - End with something positive or motivating
    - Remember that many users are neurodivergent and may struggle with executive function
    
    The user's name and their pending tasks will be provided. Create a personalized reminder message.
    """;

  public async Task<Result<string>> GenerateReminderMessageAsync(
    string personName,
    IEnumerable<string> toDoDescriptions,
    CancellationToken cancellationToken = default)
  {
    try
    {
      var taskList = string.Join("\n", toDoDescriptions.Select(t => $"- {t}"));
      var userMessage = $"Create a reminder message for {personName}. Their pending tasks are:\n{taskList}";

      var request = new ChatCompletionRequestDTO(
        ReminderSystemPrompt,
        [new ChatMessageDTO(Enums.ChatRole.User, userMessage, DateTime.UtcNow)]
      );

      var response = await apolloAIAgent.ChatAsync(request, cancellationToken);

      return Result.Ok(response);
    }
    catch (Exception ex)
    {
      return Result.Fail<string>($"Failed to generate reminder message: {ex.Message}");
    }
  }
}
