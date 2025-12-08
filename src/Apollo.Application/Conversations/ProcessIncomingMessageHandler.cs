using Apollo.AI;
using Apollo.Core.Logging;
using Apollo.Core.People;
using Apollo.Domain.People.ValueObjects;

using FluentResults;

namespace Apollo.Application.Conversations;

public sealed class ProcessIncomingMessageCommandHandler(
  IApolloAIAgent apolloAIAgent,
  ILogger<ProcessIncomingMessageCommandHandler> logger,
  IPersonService personService,
  IPersonCache personCache
) : IRequestHandler<ProcessIncomingMessageCommand, Result<Reply>>
{
  public async Task<Result<Reply>> Handle(ProcessIncomingMessageCommand request, CancellationToken cancellationToken)
  {
    try
    {
      var username = new Username(request.Message.Username, request.Message.Platform);
      var userResult = await personService.GetOrCreateAsync(username, cancellationToken);

      if (userResult.IsFailed)
      {
        return Result.Fail<Reply>($"Failed to get or create user {request.Message.Username}: {string.Join(", ", userResult.Errors.Select(e => e.Message))}");
      }

      // Check user for access
      var hasAccess = userResult.Value.HasAccess.Value;

      var cacheResult = await personCache.SetAccessAsync(username, hasAccess);

      if (cacheResult.IsFailed)
      {
        CacheLogs.UnableToSetToCache(logger, [.. cacheResult.Errors.Select(e => e.Message)]);
      }

      if (!hasAccess)
      {
        return Result.Fail<Reply>($"User {username.Value} does not have access.");
      }

      // TODO: Validate the request

      // TODO: Store the incoming message

      // TODO: Get the user's chat history here

      // TODO: Set up AI context

      // Hand message to AI here
      var response = await apolloAIAgent.ChatAsync(username, request.Message.Content, cancellationToken);

      return Result.Ok(new Reply
      {
        Content = new(response),
        CreatedOn = new(DateTime.UtcNow),
        UpdatedOn = new(DateTime.UtcNow)
      });
    }
    catch (Exception ex)
    {
      return Result.Fail<Reply>(ex.Message);
    }
  }
}
