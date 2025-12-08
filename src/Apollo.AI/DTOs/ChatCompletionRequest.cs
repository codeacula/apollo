namespace Apollo.AI.DTOs;

public sealed record ChatCompletionRequest(string SystemMessage, ICollection<ChatMessage> Messages);
