using Apollo.Domain.People.ValueObjects;
using Apollo.Domain.ToDos.Models;
using Apollo.Domain.ToDos.ValueObjects;

using FluentResults;

namespace Apollo.Application.ToDos;

public sealed record CreateToDoCommand(
  PersonId PersonId,
  Description Description,
  DateTime? ReminderDate = null
) : IRequest<Result<ToDo>>;
