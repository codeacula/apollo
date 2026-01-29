using Apollo.Domain.People.ValueObjects;
using Apollo.Domain.ToDos.Models;

using FluentResults;

namespace Apollo.Application.ToDos.Commands;

public sealed record CreateReminderCommand(
  PersonId PersonId,
  string Details,
  DateTime ReminderDate
) : IRequest<Result<Reminder>>;
