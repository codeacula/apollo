using Apollo.Domain.People.Models;

using FluentResults;

namespace Apollo.Core.ToDos;

public interface IReminderMessageGenerator
{
  Task<Result<string>> GenerateReminderMessageAsync(
    Person person,
    IEnumerable<string> toDoDescriptions,
    CancellationToken cancellationToken = default);
}
