namespace Apollo.AI.DTOs;

public sealed record ChatCompletionRequest(string SystemMessage, List<ChatMessage> Messages);
