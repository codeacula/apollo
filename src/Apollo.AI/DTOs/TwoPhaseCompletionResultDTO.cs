namespace Apollo.AI.DTOs;

public sealed record TwoPhaseCompletionResultDTO(
  string Response,
  List<string> ActionsTaken);
