using Apollo.Domain.People.ValueObjects;
using Apollo.Domain.ToDos.Models;
using Apollo.Domain.ToDos.ValueObjects;

using FluentResults;

namespace Apollo.Core.ToDos;

public interface IToDoStore
{
  Task<Result> CompleteAsync(ToDoId id, CancellationToken cancellationToken = default);
  Task<Result<ToDo>> CreateAsync(ToDoId id, PersonId personId, Description description, CancellationToken cancellationToken = default);
  Task<Result> DeleteAsync(ToDoId id, CancellationToken cancellationToken = default);
  Task<Result<ToDo>> GetAsync(ToDoId id, CancellationToken cancellationToken = default);
  Task<Result<IEnumerable<ToDo>>> GetByPersonIdAsync(PersonId personId, CancellationToken cancellationToken = default);
  Task<Result> UpdateAsync(ToDoId id, Description description, CancellationToken cancellationToken = default);
}
