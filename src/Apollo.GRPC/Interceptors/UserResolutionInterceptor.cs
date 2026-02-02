using Apollo.Application.People.Queries;
using Apollo.Domain.People.ValueObjects;
using Apollo.GRPC.Context;
using Apollo.GRPC.Contracts;
using Grpc.Core;
using Grpc.Core.Interceptors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Apollo.GRPC.Interceptors;

public class UserResolutionInterceptor : Interceptor
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
        }

        return await continuation(request, context);
    }
}
