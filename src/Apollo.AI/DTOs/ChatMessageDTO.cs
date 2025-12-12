using Apollo.AI.Enums;

namespace Apollo.AI.DTOs;

public sealed record ChatMessageDTOs(ChatRole Role, string Content, DateTime CreatedOn);
