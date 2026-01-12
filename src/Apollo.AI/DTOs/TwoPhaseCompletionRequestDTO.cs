namespace Apollo.AI.DTOs;

public sealed record TwoPhaseCompletionRequestDTO(
  string ToolCallingSystemPrompt,
  string ResponseSystemPrompt,
  string UserMessage,
  List<ChatMessageDTO> ConversationHistory,
  string? CurrentToDosContext = null);
