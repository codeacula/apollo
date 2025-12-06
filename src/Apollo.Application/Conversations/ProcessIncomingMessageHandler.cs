using Apollo.AI;
using Apollo.Core.People;
using Apollo.Domain.People.ValueObjects;

using FluentResults;

namespace Apollo.Application.Conversations;

public sealed class ProcessIncomingMessageCommandHandler(
  IApolloAIAgent apolloAIAgent,
  IPersonService personService,
  IPersonCache personCache
) : IRequestHandler<ProcessIncomingMessageCommand, Result<string>>
{
  public async Task<Result<string>> Handle(ProcessIncomingMessageCommand request, CancellationToken cancellationToken)
  {
    try
    {
      var username = new Username(request.Message.Username, request.Message.Platform);
      var userResult = await personService.GetOrCreateAsync(username, cancellationToken);

      if (userResult.IsFailed)
      {
        return Result.Fail<string>($"Failed to get or create user {request.Message.Username}: {string.Join(", ", userResult.Errors.Select(e => e.Message))}");
      }

      // Check user for access
      var hasAccess = userResult.Value.HasAccess.Value;

      var cacheResult = await personCache.SetAccessAsync(username, hasAccess);

      if (cacheResult.IsFailed)
      {
        // TODO: Log cache set failure
      }

      if (!hasAccess)
      {
        return Result.Fail<string>($"User {username.Value} does not have access.");
      }

      // Hand message to AI here
      var response = await apolloAIAgent.ChatAsync(username, request.Message.Content, cancellationToken);
      return Result.Ok(response);
    }
    catch (Exception ex)
    {
      return Result.Fail<string>(ex.Message);
    }
  }
}
