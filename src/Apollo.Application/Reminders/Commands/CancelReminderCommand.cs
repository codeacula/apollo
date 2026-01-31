using Apollo.Domain.People.ValueObjects;
using Apollo.Domain.ToDos.ValueObjects;

using FluentResults;

namespace Apollo.Application.Reminders.Commands;

public sealed record CancelReminderCommand(
  PersonId PersonId,
  ReminderId ReminderId
) : IRequest<Result>;
