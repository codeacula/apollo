using Apollo.Core.ToDos;
using Apollo.Database.ToDos.Events;
using Apollo.Domain.People.ValueObjects;
using Apollo.Domain.ToDos.Models;
using Apollo.Domain.ToDos.ValueObjects;

using FluentResults;

using Marten;

namespace Apollo.Database.ToDos;

public sealed class ToDoStore(IDocumentSession session, TimeProvider timeProvider) : IToDoStore
{
  public async Task<Result> CompleteAsync(ToDoId id, CancellationToken cancellationToken = default)
  {
    try
    {
      var time = timeProvider.GetUtcNow().DateTime;
      _ = session.Events.Append(id.Value, new ToDoCompletedEvent(id.Value, time));

      await session.SaveChangesAsync(cancellationToken);

      return Result.Ok();
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }

  public async Task<Result<ToDo>> CreateAsync(ToDoId id, PersonId personId, Description description, CancellationToken cancellationToken = default)
  {
    try
    {
      var time = timeProvider.GetUtcNow().DateTime;
      var ev = new ToDoCreatedEvent(id.Value, personId.Value, description.Value, time);

      _ = session.Events.StartStream<DbToDo>(id.Value, [ev]);
      await session.SaveChangesAsync(cancellationToken);

      var newToDo = await session.Events.AggregateStreamAsync<DbToDo>(id.Value, token: cancellationToken);

      return newToDo is null ? Result.Fail<ToDo>("Failed to create new toDo") : Result.Ok((ToDo)newToDo);
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }

  public async Task<Result> DeleteAsync(ToDoId id, CancellationToken cancellationToken = default)
  {
    try
    {
      var time = timeProvider.GetUtcNow().DateTime;
      _ = session.Events.Append(id.Value, new ToDoDeletedEvent(id.Value, time));

      await session.SaveChangesAsync(cancellationToken);

      return Result.Ok();
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }

  public async Task<Result<ToDo>> GetAsync(ToDoId id, CancellationToken cancellationToken = default)
  {
    try
    {
      var dbToDo = await session.Query<DbToDo>().FirstOrDefaultAsync(t => t.Id == id.Value && !t.IsDeleted, cancellationToken);
      return dbToDo is null ? Result.Fail<ToDo>($"ToDo with ID {id.Value} not found") : Result.Ok((ToDo)dbToDo);
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }

  public async Task<Result<IEnumerable<ToDo>>> GetByPersonIdAsync(PersonId personId, CancellationToken cancellationToken = default)
  {
    try
    {
      var dbToDos = await session.Query<DbToDo>()
        .Where(t => t.PersonId == personId.Value && !t.IsDeleted && !t.IsCompleted)
        .ToListAsync(cancellationToken);

      var toDos = dbToDos.Select(t => (ToDo)t);
      return Result.Ok(toDos);
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }

  public async Task<Result<IEnumerable<ToDo>>> GetDueTasksAsync(DateTime beforeTime, CancellationToken cancellationToken = default)
  {
    try
    {
      var dbToDos = await session.Query<DbToDo>()
        .Where(t => !t.IsDeleted && !t.IsCompleted && t.ReminderDate.HasValue && t.ReminderDate.Value <= beforeTime)
        .ToListAsync(cancellationToken);

      var todos = dbToDos.Select(t => (ToDo)t);
      return Result.Ok(todos);
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }

  public async Task<Result> SetReminderAsync(ToDoId id, DateTime reminderDate, CancellationToken cancellationToken = default)
  {
    try
    {
      var time = timeProvider.GetUtcNow().DateTime;
      _ = session.Events.Append(id.Value, new ToDoReminderSetEvent(id.Value, reminderDate, time));

      await session.SaveChangesAsync(cancellationToken);

      return Result.Ok();
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }

  public async Task<Result> UpdateAsync(ToDoId id, Description description, CancellationToken cancellationToken = default)
  {
    try
    {
      var time = timeProvider.GetUtcNow().DateTime;
      _ = session.Events.Append(id.Value, new ToDoUpdatedEvent(id.Value, description.Value, time));

      await session.SaveChangesAsync(cancellationToken);

      return Result.Ok();
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }
}
