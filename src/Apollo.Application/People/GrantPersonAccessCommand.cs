using Apollo.Application.People.Notifications;
using Apollo.Core.People;
using Apollo.Domain.People.ValueObjects;

using FluentResults;

namespace Apollo.Application.People;

public sealed record GrantPersonAccessCommand(PersonId PersonId) : IRequest<Result>;

public sealed class GrantPersonAccessCommandHandler(IPersonStore personStore, IMediator mediator)
  : IRequestHandler<GrantPersonAccessCommand, Result>
{
  public async Task<Result> Handle(GrantPersonAccessCommand request, CancellationToken cancellationToken)
  {
    var result = await personStore.GrantAccessAsync(request.PersonId, cancellationToken);
    if (result.IsSuccess)
    {
      await mediator.Publish(new PersonAccessGrantedNotification(), cancellationToken);
    }

    return result;
  }
}
