using FluentResults;

namespace Apollo.Core.ToDos;

public interface IReminderMessageGenerator
{
  Task<Result<string>> GenerateReminderMessageAsync(
    string personName,
    IEnumerable<string> toDoDescriptions,
    CancellationToken cancellationToken = default);
}
