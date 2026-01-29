using Apollo.Application.ToDos.Commands;
using Apollo.Core;
using Apollo.Core.ToDos;
using Apollo.Domain.Common.Enums;
using Apollo.Domain.ToDos.Models;
using Apollo.Domain.ToDos.ValueObjects;

using FluentResults;

namespace Apollo.Application.ToDos.Handlers;

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

      // Default to Green (Level.Green = 1) if not specified
      var priority = request.Priority ?? new Priority(Level.Green);
      var energy = request.Energy ?? new Energy(Level.Green);
      var interest = request.Interest ?? new Interest(Level.Green);

      var result = await toDoStore.CreateAsync(toDoId, request.PersonId, request.Description, priority, energy, interest, cancellationToken);

      if (result.IsFailed)
      {
        return result;
      }

      if (request.ReminderDate.HasValue)
      {
        var jobResult = await toDoReminderScheduler.GetOrCreateJobAsync(request.ReminderDate.Value, cancellationToken);
        if (jobResult.IsFailed)
        {
          return Result.Fail<ToDo>($"To-Do created but failed to schedule reminder job: {jobResult.GetErrorMessages()}");
        }

        var reminderId = new ReminderId(Guid.NewGuid());
        var reminderDetails = new Details(request.Description.Value);
        var reminderTime = new ReminderTime(request.ReminderDate.Value);

        var createReminderResult = await reminderStore.CreateAsync(
          reminderId,
          request.PersonId,
          reminderDetails,
          reminderTime,
          jobResult.Value,
          cancellationToken);

        if (createReminderResult.IsFailed)
        {
          return Result.Ok(result.Value)
            .WithError($"To-Do created but failed to create reminder: {createReminderResult.GetErrorMessages()}");
        }

        var linkResult = await reminderStore.LinkToToDoAsync(reminderId, toDoId, cancellationToken);
        if (linkResult.IsFailed)
        {
          return Result.Ok(result.Value)
            .WithError($"To-Do and reminder created but failed to link them: {linkResult.GetErrorMessages()}");
        }

        // Ensure the job exists *after* the reminder is persisted to avoid a delete/create race.
        var ensureJobResult = await toDoReminderScheduler.GetOrCreateJobAsync(request.ReminderDate.Value, cancellationToken);
        if (ensureJobResult.IsFailed)
        {
          return Result.Fail<ToDo>($"To-Do created but failed to ensure reminder job exists: {ensureJobResult.GetErrorMessages()}");
        }
      }

      return result.Value;
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }
}
