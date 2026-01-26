using Apollo.Domain.ToDos.Models;
using Apollo.Domain.ToDos.ValueObjects;

using FluentResults;

namespace Apollo.Application.ToDos.Commands;

public sealed record AddReminderCommand(
  ToDoId ToDoId,
  DateTime ReminderDate
) : IRequest<Result<Reminder>>;
