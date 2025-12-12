namespace Apollo.AI.DTOs;

public sealed record ChatCompletionRequestDTO(string SystemMessage, List<ChatMessageDTO> Messages);
