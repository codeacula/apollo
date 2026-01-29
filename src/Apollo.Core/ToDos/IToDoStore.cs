using Apollo.Domain.People.ValueObjects;
using Apollo.Domain.ToDos.Models;
using Apollo.Domain.ToDos.ValueObjects;

using FluentResults;

namespace Apollo.Core.ToDos;

public interface IToDoStore
{
  Task<Result> CompleteAsync(ToDoId id, CancellationToken cancellationToken = default);
  Task<Result<ToDo>> CreateAsync(ToDoId id, PersonId personId, Description description, Priority priority, Energy energy, Interest interest, CancellationToken cancellationToken = default);
  Task<Result> DeleteAsync(ToDoId id, CancellationToken cancellationToken = default);
  Task<Result<ToDo>> GetAsync(ToDoId id, CancellationToken cancellationToken = default);
  Task<Result<IEnumerable<ToDo>>> GetByPersonIdAsync(PersonId personId, bool includeCompleted = false, CancellationToken cancellationToken = default);
  Task<Result> UpdateAsync(ToDoId id, Description description, CancellationToken cancellationToken = default);
  Task<Result> UpdatePriorityAsync(ToDoId toDoId, Priority priority, CancellationToken cancellationToken = default);
  Task<Result> UpdateEnergyAsync(ToDoId toDoId, Energy energy, CancellationToken cancellationToken = default);
  Task<Result> UpdateInterestAsync(ToDoId toDoId, Interest interest, CancellationToken cancellationToken = default);
}
