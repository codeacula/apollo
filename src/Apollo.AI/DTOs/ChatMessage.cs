using Apollo.AI.Enums;

namespace Apollo.AI.DTOs;

public sealed record ChatMessage(ChatRole Role, string Content, DateTime CreatedOn);
