using Apollo.Domain.ToDos.ValueObjects;

using FluentResults;

namespace Apollo.Application.ToDos.Commands;

public sealed record RemoveReminderCommand(
  ToDoId ToDoId,
  ReminderId ReminderId
) : IRequest<Result>;
