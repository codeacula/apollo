using Apollo.Core;
using Apollo.Core.ToDos;
using Apollo.Domain.Common.Enums;
using Apollo.Domain.People.ValueObjects;
using Apollo.Domain.ToDos.Models;
using Apollo.Domain.ToDos.ValueObjects;

using FluentResults;

namespace Apollo.Application.ToDos;

public sealed record CreateToDoCommand(
  PersonId PersonId,
  Description Description,
  DateTime? ReminderDate = null,
  Priority? Priority = null,
  Energy? Energy = null,
  Interest? Interest = null
) : IRequest<Result<ToDo>>;

public sealed class CreateToDoCommandHandler(
  IToDoStore toDoStore,
  IReminderStore reminderStore,
  IToDoReminderScheduler toDoReminderScheduler) : IRequestHandler<CreateToDoCommand, Result<ToDo>>
{
  public async Task<Result<ToDo>> Handle(CreateToDoCommand request, CancellationToken cancellationToken)
  {
    try
    {
      var toDoId = new ToDoId(Guid.NewGuid());
      var (priority, energy, interest) = ResolveDefaults(request.Priority, request.Energy, request.Interest);

      var createResult = await CreateToDoEntityAsync(toDoId, request.PersonId, request.Description, priority, energy, interest, cancellationToken);
      if (createResult.IsFailed)
      {
        return createResult;
      }

      if (request.ReminderDate.HasValue)
      {
        var reminderResult = await ScheduleReminderForToDoAsync(toDoId, request.PersonId, request.Description, request.ReminderDate.Value, cancellationToken);
        if (reminderResult.IsFailed)
        {
          return Result.Ok(createResult.Value).WithError(reminderResult.GetErrorMessages());
        }
      }

      return createResult.Value;
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }

  private static (Priority, Energy, Interest) ResolveDefaults(Priority? priority, Energy? energy, Interest? interest)
  {
    return (
      priority ?? new Priority(Level.Green),
      energy ?? new Energy(Level.Green),
      interest ?? new Interest(Level.Green)
    );
  }

  private async Task<Result<ToDo>> CreateToDoEntityAsync(
    ToDoId id,
    PersonId personId,
    Description description,
    Priority priority,
    Energy energy,
    Interest interest,
    CancellationToken cancellationToken)
  {
    return await toDoStore.CreateAsync(id, personId, description, priority, energy, interest, cancellationToken);
  }

  private async Task<Result> ScheduleReminderForToDoAsync(
    ToDoId toDoId,
    PersonId personId,
    Description description,
    DateTime reminderDate,
    CancellationToken cancellationToken)
  {
    var jobResult = await toDoReminderScheduler.GetOrCreateJobAsync(reminderDate, cancellationToken);
    if (jobResult.IsFailed)
    {
      return Result.Fail($"To-Do created but failed to schedule reminder job: {jobResult.GetErrorMessages()}");
    }

    var reminderId = new ReminderId(Guid.NewGuid());
    var reminderDetails = new Details(description.Value);
    var reminderTime = new ReminderTime(reminderDate);

    var createReminderResult = await reminderStore.CreateAsync(
      reminderId,
      personId,
      reminderDetails,
      reminderTime,
      jobResult.Value,
      cancellationToken);

    if (createReminderResult.IsFailed)
    {
      return Result.Fail($"To-Do created but failed to create reminder: {createReminderResult.GetErrorMessages()}");
    }

    var linkResult = await reminderStore.LinkToToDoAsync(reminderId, toDoId, cancellationToken);
    if (linkResult.IsFailed)
    {
      return Result.Fail($"To-Do and reminder created but failed to link them: {linkResult.GetErrorMessages()}");
    }

    // Ensure the job exists *after* the reminder is persisted to avoid a delete/create race.
    var ensureJobResult = await toDoReminderScheduler.GetOrCreateJobAsync(reminderDate, cancellationToken);
    return ensureJobResult.IsFailed
      ? Result.Fail($"To-Do created but failed to ensure reminder job exists: {ensureJobResult.GetErrorMessages()}")
      : Result.Ok();
  }
}
