using Apollo.Core.Configuration;
using Apollo.Domain.Common.Enums;
using Apollo.Domain.People.Models;
using Apollo.GRPC.Attributes;
using Apollo.GRPC.Context;

using Grpc.Core;
using Grpc.Core.Interceptors;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Apollo.GRPC.Interceptors;

public sealed class AuthorizationInterceptor(IServiceScopeFactory serviceScopeFactory) : Interceptor
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
      (_, true) when person == null || !await IsSuperAdminAsync(person) => throw new RpcException(new Status(StatusCode.PermissionDenied, "Super Admin access required.")),
      _ => 0
    };

    return await continuation(request, context);
  }

  private async Task<bool> IsSuperAdminAsync(Person person)
  {
    if (person.PlatformId.Platform != Platform.Discord)
    {
      return false;
    }

    using var scope = serviceScopeFactory.CreateScope();
    var configStore = scope.ServiceProvider.GetRequiredService<IConfigurationStore>();
    var configResult = await configStore.GetAsync();

    if (configResult.IsFailed)
    {
      return false;
    }

    var superAdminDiscordUserId = configResult.Value.SuperAdminDiscordUserId;
    return !string.IsNullOrWhiteSpace(superAdminDiscordUserId)
      && string.Equals(person.PlatformId.PlatformUserId, superAdminDiscordUserId, StringComparison.OrdinalIgnoreCase);
  }
}
