using Apollo.AI;
using Apollo.Application.Commands;
using Apollo.Core.Infrastructure.Cache;
using Apollo.Core.Infrastructure.Services;

using FluentResults;

namespace Apollo.Application.Handlers;

public sealed class ProcessIncomingMessageHandler(
  IApolloAIAgent apolloAIAgent,
  IApolloUserService apolloUserService,
  IUserCache userCache
) : IRequestHandler<ProcessIncomingMessage, Result<string>>
{
  public async Task<Result<string>> Handle(ProcessIncomingMessage request, CancellationToken cancellationToken)
  {
    try
    {
      // Get the user
      var userResult = await apolloUserService.GetOrCreateUserAsync(request.Message.Username, cancellationToken);

      if (userResult.IsFailed)
      {
        return Result.Fail<string>($"Failed to get or create user {request.Message.Username}: {string.Join(", ", userResult.Errors.Select(e => e.Message))}");
      }

      // Check user for access
      var hasAccess = userResult.Value.HasAccess.Value;

      var cacheResult = await userCache.SetUserAccessAsync(request.Message.Username, hasAccess, cancellationToken);

      if (cacheResult.IsFailed)
      {
        // TODO: Log cache set failure
      }

      if (!hasAccess)
      {
        return Result.Fail<string>($"User {request.Message.Username} does not have access.");
      }

      // Hand message to AI here
      var response = await apolloAIAgent.ChatAsync(request.Message.Username, request.Message.Content, cancellationToken);
      return Result.Ok(response);
    }
    catch (Exception ex)
    {
      return Result.Fail<string>(ex.Message);
    }
  }
}
