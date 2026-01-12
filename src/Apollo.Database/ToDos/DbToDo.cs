using Apollo.Database.ToDos.Events;
using Apollo.Domain.ToDos.Models;

using JasperFx.Events;

namespace Apollo.Database.ToDos;

public sealed record DbToDo
{
  public required Guid Id { get; init; }
  public required Guid PersonId { get; init; }
  public required string Description { get; init; }
  public DateTime? DueDate { get; init; }
  public bool IsCompleted { get; init; }
  public bool IsDeleted { get; init; }
  public DateTime CreatedOn { get; init; }
  public DateTime UpdatedOn { get; init; }

  public static explicit operator ToDo(DbToDo dbToDo)
  {
    return new()
    {
      Id = new(dbToDo.Id),
      PersonId = new(dbToDo.PersonId),
      Description = new(dbToDo.Description),
      Priority = new(0),
      Energy = new(0),
      Interest = new(0),
      DueDate = dbToDo.DueDate.HasValue ? new(dbToDo.DueDate.Value) : null,
      CreatedOn = new(dbToDo.CreatedOn),
      UpdatedOn = new(dbToDo.UpdatedOn),
      Reminders = [],
    };
  }

  public static DbToDo Create(IEvent<ToDoCreatedEvent> ev)
  {
    var eventData = ev.Data;

    return new()
    {
      Id = eventData.Id,
      PersonId = eventData.PersonId,
      Description = eventData.Description,
      IsCompleted = false,
      IsDeleted = false,
      DueDate = null,
      CreatedOn = eventData.CreatedOn,
      UpdatedOn = eventData.CreatedOn
    };
  }

  public static DbToDo Apply(IEvent<ToDoUpdatedEvent> ev, DbToDo toDo)
  {
    return toDo with
    {
      Description = ev.Data.Description,
      UpdatedOn = ev.Data.UpdatedOn
    };
  }

  public static DbToDo Apply(IEvent<ToDoCompletedEvent> ev, DbToDo toDo)
  {
    return toDo with
    {
      IsCompleted = true,
      UpdatedOn = ev.Data.CompletedOn
    };
  }

  public static DbToDo Apply(IEvent<ToDoDeletedEvent> ev, DbToDo toDo)
  {
    return toDo with
    {
      IsDeleted = true,
      UpdatedOn = ev.Data.DeletedOn
    };
  }
}
