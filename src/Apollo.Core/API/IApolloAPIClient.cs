using Apollo.Core.Conversations;
using Apollo.Core.ToDos.Requests;
using Apollo.Core.ToDos.Responses;
using Apollo.Domain.Common.Enums;
using Apollo.Domain.ToDos.Models;

using FluentResults;

namespace Apollo.Core.API;

public interface IApolloAPIClient
{
  Task<Result<ToDo>> CreateToDoAsync(CreateToDoRequest request);
  Task<Result<IEnumerable<ToDoSummary>>> GetToDosAsync(string username, Platform platform);
  Task<Result<string>> SendMessageAsync(NewMessageRequest request);
}
