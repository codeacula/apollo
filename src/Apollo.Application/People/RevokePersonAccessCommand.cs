using Apollo.Application.People.Notifications;
using Apollo.Core.People;
using Apollo.Domain.People.ValueObjects;

using FluentResults;

namespace Apollo.Application.People;

public sealed record RevokePersonAccessCommand(PersonId PersonId) : IRequest<Result>;

public sealed class RevokePersonAccessCommandHandler(IPersonStore personStore, IMediator mediator)
  : IRequestHandler<RevokePersonAccessCommand, Result>
{
  public async Task<Result> Handle(RevokePersonAccessCommand request, CancellationToken cancellationToken)
  {
    var result = await personStore.RevokeAccessAsync(request.PersonId, cancellationToken);
    if (result.IsSuccess)
    {
      await mediator.Publish(new PersonAccessRevokedNotification(), cancellationToken);
    }

    return result;
  }
}
