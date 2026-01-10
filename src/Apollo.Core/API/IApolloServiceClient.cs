using Apollo.Core.Conversations;
using Apollo.Core.ToDos.Requests;
using Apollo.Core.ToDos.Responses;
using Apollo.Domain.People.ValueObjects;
using Apollo.Domain.ToDos.Models;

using FluentResults;

namespace Apollo.Core.API;

public interface IApolloServiceClient
{
  Task<Result<ToDo>> CreateToDoAsync(CreateToDoRequest request);
  Task<Result<IEnumerable<ToDoSummary>>> GetToDosAsync(PlatformId platformId, bool includeCompleted = false);
  Task<Result<string>> SendMessageAsync(NewMessageRequest request);
}
