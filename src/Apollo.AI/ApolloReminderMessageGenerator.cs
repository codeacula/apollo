using Apollo.AI.DTOs;
using Apollo.AI.Enums;
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
    - Keep the message concise (2-3 sentences)
    - Include a cat-themed touch when appropriate (purrs, meows, gentle nudges)
    - Focus on being encouraging, not nagging
    - Mention the tasks naturally in conversation, don't use bullet points or numbered lists
    - End with something positive or motivating
    - Remember that many users are neurodivergent and may struggle with executive function
    - Do NOT wrap your response in quotes - just write the message directly
    
    The user's name and their pending tasks will be provided. Create a personalized reminder message.
    """;

  public async Task<Result<string>> GenerateReminderMessageAsync(
    string personName,
    IEnumerable<string> toDoDescriptions,
    CancellationToken cancellationToken = default)
  {
    try
    {
      var taskList = string.Join(", ", toDoDescriptions);
      var userMessage = $"Create a reminder message for {personName}. Their pending tasks are: {taskList}";

      var result = await apolloAIAgent
        .CreateRequest()
        .WithSystemPrompt(ReminderSystemPrompt)
        .WithTemperature(0.8)
        .WithMessage(new ChatMessageDTO(ChatRole.User, userMessage, DateTime.UtcNow))
        .WithToolCalling(enabled: false)
        .ExecuteAsync(cancellationToken);

      if (!result.Success)
      {
        return Result.Fail<string>($"Failed to generate reminder message: {result.ErrorMessage}");
      }

      // Remove any surrounding quotes the LLM might add
      return Result.Ok(result.Content.Trim().Trim('"'));
    }
    catch (Exception ex)
    {
      return Result.Fail<string>($"Failed to generate reminder message: {ex.Message}");
    }
  }
}
