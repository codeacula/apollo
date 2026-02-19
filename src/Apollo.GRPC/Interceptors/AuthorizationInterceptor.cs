using Apollo.Core.People;
using Apollo.Domain.Common.Enums;
using Apollo.Domain.People.Models;
using Apollo.GRPC.Attributes;
using Apollo.GRPC.Context;

using Grpc.Core;
using Grpc.Core.Interceptors;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Apollo.GRPC.Interceptors;

public class AuthorizationInterceptor(SuperAdminConfig superAdminConfig) : Interceptor
{

  public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
      TRequest request,
      ServerCallContext context,
      UnaryServerMethod<TRequest, TResponse> continuation)
  {
    var httpContext = context.GetHttpContext();
    var endpoint = httpContext?.GetEndpoint();

    if (endpoint == null)
    {
      return await continuation(request, context);
    }

    var metadata = endpoint.Metadata;
    var requireAccess = metadata.GetMetadata<RequireAccessAttribute>() != null;
    var requireSuperAdmin = metadata.GetMetadata<RequireSuperAdminAttribute>() != null;

    if (!requireAccess && !requireSuperAdmin)
    {
      return await continuation(request, context);
    }

    var userContext = httpContext!.RequestServices.GetService<IUserContext>();
    var person = userContext?.Person;

    _ = (requireAccess, requireSuperAdmin) switch
    {
      (true, _) when person?.HasAccess.Value != true => throw new RpcException(new Status(StatusCode.PermissionDenied, "Access denied.")),
      (_, true) when person == null || !IsSuperAdmin(person) => throw new RpcException(new Status(StatusCode.PermissionDenied, "Super Admin access required.")),
      _ => 0
    };

    return await continuation(request, context);
  }

  private bool IsSuperAdmin(Person person)
  {
    return person.PlatformId.Platform switch
    {
      Platform.Discord when !string.IsNullOrWhiteSpace(superAdminConfig.DiscordUserId)
        => string.Equals(person.PlatformId.PlatformUserId, superAdminConfig.DiscordUserId, StringComparison.OrdinalIgnoreCase),
      _ => false
    };
  }
}
