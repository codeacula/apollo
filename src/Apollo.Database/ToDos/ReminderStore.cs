using Apollo.Core;
using Apollo.Core.ToDos;
using Apollo.Database.ToDos.Events;
using Apollo.Domain.People.ValueObjects;
using Apollo.Domain.ToDos.Models;
using Apollo.Domain.ToDos.ValueObjects;

using FluentResults;

using Marten;

namespace Apollo.Database.ToDos;

public sealed class ReminderStore(IDocumentSession session, TimeProvider timeProvider) : IReminderStore
{
  public async Task<Result<Reminder>> CreateAsync(
    ReminderId id,
    PersonId personId,
    Details details,
    ReminderTime reminderTime,
    QuartzJobId quartzJobId,
    CancellationToken cancellationToken = default)
  {
    try
    {
      var time = timeProvider.GetUtcDateTime();
      var ev = new ReminderCreatedEvent(id.Value, personId.Value, details.Value, reminderTime.Value, quartzJobId.Value, time);

      _ = session.Events.StartStream<DbReminder>(id.Value, [ev]);
      await session.SaveChangesAsync(cancellationToken);

      var newReminder = await session.Events.AggregateStreamAsync<DbReminder>(id.Value, token: cancellationToken);

      return newReminder is null
        ? Result.Fail<Reminder>("Failed to create new reminder")
        : Result.Ok((Reminder)newReminder);
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }

  public async Task<Result<Reminder>> GetAsync(ReminderId id, CancellationToken cancellationToken = default)
  {
    try
    {
      var dbReminder = await session.Query<DbReminder>()
        .FirstOrDefaultAsync(r => r.Id == id.Value && !r.IsDeleted, cancellationToken);

      return dbReminder is null
        ? Result.Fail<Reminder>($"Reminder with ID {id.Value} not found")
        : Result.Ok((Reminder)dbReminder);
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }

  public async Task<Result<IEnumerable<Reminder>>> GetByToDoIdAsync(ToDoId toDoId, CancellationToken cancellationToken = default)
  {
    try
    {
      var links = await session.Query<DbToDoReminder>()
        .Where(l => l.ToDoId == toDoId.Value && !l.IsDeleted)
        .ToListAsync(cancellationToken);

      var reminderIds = links.Select(l => l.ReminderId).ToList();

      if (reminderIds.Count == 0)
      {
        return Result.Ok(Enumerable.Empty<Reminder>());
      }

      var dbReminders = await session.Query<DbReminder>()
        .Where(r => reminderIds.Contains(r.Id) && !r.IsDeleted)
        .ToListAsync(cancellationToken);

      var reminders = dbReminders.Select(r => (Reminder)r);
      return Result.Ok(reminders);
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }

  public async Task<Result<IEnumerable<Reminder>>> GetByQuartzJobIdAsync(QuartzJobId quartzJobId, CancellationToken cancellationToken = default)
  {
    try
    {
      var dbReminders = await session.Query<DbReminder>()
        .Where(r => !r.IsDeleted && r.QuartzJobId == quartzJobId.Value)
        .ToListAsync(cancellationToken);

      var reminders = dbReminders.Select(r => (Reminder)r);
      return Result.Ok(reminders);
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }

  public async Task<Result> LinkToToDoAsync(ReminderId reminderId, ToDoId toDoId, CancellationToken cancellationToken = default)
  {
    try
    {
      var time = timeProvider.GetUtcDateTime();
      var linkId = Guid.NewGuid();
      var ev = new ToDoReminderLinkedEvent(linkId, toDoId.Value, reminderId.Value, time);

      _ = session.Events.StartStream<DbToDoReminder>(linkId, [ev]);
      await session.SaveChangesAsync(cancellationToken);

      return Result.Ok();
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }

  public async Task<Result> UnlinkFromToDoAsync(ReminderId reminderId, ToDoId toDoId, CancellationToken cancellationToken = default)
  {
    try
    {
      var link = await session.Query<DbToDoReminder>()
        .FirstOrDefaultAsync(l => l.ToDoId == toDoId.Value && l.ReminderId == reminderId.Value && !l.IsDeleted, cancellationToken);

      if (link is null)
      {
        return Result.Fail($"Link between ToDo {toDoId.Value} and Reminder {reminderId.Value} not found");
      }

      var time = timeProvider.GetUtcDateTime();
      var ev = new ToDoReminderUnlinkedEvent(link.Id, toDoId.Value, reminderId.Value, time);

      _ = session.Events.Append(link.Id, ev);
      await session.SaveChangesAsync(cancellationToken);

      return Result.Ok();
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }

  public async Task<Result> MarkAsSentAsync(ReminderId id, CancellationToken cancellationToken = default)
  {
    try
    {
      var time = timeProvider.GetUtcDateTime();
      var ev = new ReminderSentEvent(id.Value, time);

      _ = session.Events.Append(id.Value, ev);
      await session.SaveChangesAsync(cancellationToken);

      return Result.Ok();
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }

  public async Task<Result> AcknowledgeAsync(ReminderId id, CancellationToken cancellationToken = default)
  {
    try
    {
      var time = timeProvider.GetUtcDateTime();
      var ev = new ReminderAcknowledgedEvent(id.Value, time);

      _ = session.Events.Append(id.Value, ev);
      await session.SaveChangesAsync(cancellationToken);

      return Result.Ok();
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }

  public async Task<Result> DeleteAsync(ReminderId id, CancellationToken cancellationToken = default)
  {
    try
    {
      var time = timeProvider.GetUtcDateTime();
      var ev = new ReminderDeletedEvent(id.Value, time);

      _ = session.Events.Append(id.Value, ev);
      await session.SaveChangesAsync(cancellationToken);

      return Result.Ok();
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }

  public async Task<Result<IEnumerable<ToDoId>>> GetLinkedToDoIdsAsync(ReminderId reminderId, CancellationToken cancellationToken = default)
  {
    try
    {
      var links = await session.Query<DbToDoReminder>()
        .Where(l => l.ReminderId == reminderId.Value && !l.IsDeleted)
        .ToListAsync(cancellationToken);

      var toDoIds = links.Select(l => new ToDoId(l.ToDoId));
      return Result.Ok(toDoIds);
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }

  public async Task<Result<IEnumerable<ReminderId>>> GetLinkedReminderIdsAsync(ToDoId toDoId, CancellationToken cancellationToken = default)
  {
    try
    {
      var links = await session.Query<DbToDoReminder>()
        .Where(l => l.ToDoId == toDoId.Value && !l.IsDeleted)
        .ToListAsync(cancellationToken);

      var reminderIds = links.Select(l => new ReminderId(l.ReminderId));
      return Result.Ok(reminderIds);
    }
    catch (Exception ex)
    {
      return Result.Fail(ex.Message);
    }
  }
}
