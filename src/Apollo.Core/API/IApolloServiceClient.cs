using Apollo.Core.Conversations;
using Apollo.Core.ToDos.Requests;
using Apollo.Core.ToDos.Responses;
using Apollo.Domain.People.ValueObjects;
using Apollo.Domain.ToDos.Models;

using FluentResults;

namespace Apollo.Core.API;

public interface IApolloServiceClient
{
  Task<Result<ToDo>> CreateToDoAsync(CreateToDoRequest request, CancellationToken cancellationToken = default);
  Task<Result<IEnumerable<ToDoSummary>>> GetToDosAsync(PlatformId platformId, bool includeCompleted = false, CancellationToken cancellationToken = default);
  Task<Result<DailyPlanResponse>> GetDailyPlanAsync(PlatformId platformId, CancellationToken cancellationToken = default);
  Task<Result<string>> GrantAccessAsync(PlatformId adminPlatformId, PlatformId targetPlatformId, CancellationToken cancellationToken = default);
  Task<Result<string>> RevokeAccessAsync(PlatformId adminPlatformId, PlatformId targetPlatformId, CancellationToken cancellationToken = default);
  Task<Result<string>> SendMessageAsync(NewMessageRequest request, CancellationToken cancellationToken = default);
}
