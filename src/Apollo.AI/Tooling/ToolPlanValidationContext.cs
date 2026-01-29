using Apollo.AI.DTOs;

namespace Apollo.AI.Tooling;

public sealed record ToolPlanValidationContext(
  IDictionary<string, object> Plugins,
  IReadOnlyList<ChatMessageDTO> ConversationHistory,
  IReadOnlyCollection<string> ActiveTodoIds);
