using Apollo.Application.People;
using Apollo.Core.People;
using Apollo.Domain.Common.Enums;
using Apollo.Domain.People.Models;
using Apollo.Domain.People.ValueObjects;
using Apollo.GRPC.Context;
using Apollo.GRPC.Contracts;

using Grpc.Core;
using Grpc.Core.Interceptors;

using MediatR;

using Microsoft.Extensions.DependencyInjection;

namespace Apollo.GRPC.Interceptors;

public sealed class UserResolutionInterceptor : Interceptor
{
  public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
      TRequest request,
      ServerCallContext context,
      UnaryServerMethod<TRequest, TResponse> continuation)
  {
    if (request is not IAuthenticatedRequest authRequest)
    {
      return await continuation(request, context);
    }

    var httpContext = context.GetHttpContext();
    if (httpContext == null)
    {
      return await continuation(request, context);
    }

    var services = httpContext.RequestServices;
    var mediator = services.GetRequiredService<IMediator>();
    var userContext = services.GetRequiredService<IUserContext>();

    var platformId = new PlatformId(authRequest.Username, authRequest.PlatformUserId, authRequest.Platform);
    var query = new GetOrCreatePersonByPlatformIdQuery(platformId);

    var result = await mediator.Send(query);

    if (result.IsSuccess)
    {
      userContext.Person = result.Value;
      await EnsureNotificationChannelAsync(services, authRequest, result.Value);
    }

    return await continuation(request, context);
  }

  private static async Task EnsureNotificationChannelAsync(
    IServiceProvider services,
    IAuthenticatedRequest request,
    Person person)
  {
    var channelType = request.Platform switch
    {
      Platform.Discord => NotificationChannelType.Discord,
      _ => (NotificationChannelType?)null
    };

    if (!channelType.HasValue)
    {
      return;
    }

    var personStore = services.GetRequiredService<IPersonStore>();
    var channel = new NotificationChannel(channelType.Value, request.PlatformUserId, isEnabled: true);
    _ = await personStore.EnsureNotificationChannelAsync(person, channel);
  }
}
