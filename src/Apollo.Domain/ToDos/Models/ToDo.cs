using Apollo.Domain.Common.ValueObjects;
using Apollo.Domain.People.ValueObjects;
using Apollo.Domain.ToDos.ValueObjects;

namespace Apollo.Domain.ToDos.Models;

public record ToDo
{
  public required ToDoId Id { get; init; }
  public required PersonId PersonId { get; init; }
  public required Description Description { get; init; }
  public required Priority Priority { get; init; }
  public required Energy Energy { get; init; }
  public required Interest Interest { get; init; }
  public ICollection<Reminder> Reminders { get; init; } = [];
  public DueDate? DueDate { get; init; }
  public CreatedOn CreatedOn { get; init; }
  public UpdatedOn UpdatedOn { get; init; }
}
