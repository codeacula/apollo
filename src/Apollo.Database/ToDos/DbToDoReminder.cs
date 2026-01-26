using Apollo.Database.ToDos.Events;

using JasperFx.Events;

namespace Apollo.Database.ToDos;

public sealed record DbToDoReminder
{
  public required Guid Id { get; init; }
  public required Guid ToDoId { get; init; }
  public required Guid ReminderId { get; init; }
  public bool IsDeleted { get; init; }
  public DateTime CreatedOn { get; init; }
  public DateTime UpdatedOn { get; init; }

  public static DbToDoReminder Create(IEvent<ToDoReminderLinkedEvent> ev)
  {
    var eventData = ev.Data;

    return new()
    {
      Id = eventData.Id,
      ToDoId = eventData.ToDoId,
      ReminderId = eventData.ReminderId,
      IsDeleted = false,
      CreatedOn = eventData.LinkedOn,
      UpdatedOn = eventData.LinkedOn
    };
  }

  public static DbToDoReminder Apply(IEvent<ToDoReminderUnlinkedEvent> ev, DbToDoReminder link)
  {
    return link with
    {
      IsDeleted = true,
      UpdatedOn = ev.Data.UnlinkedOn
    };
  }
}
