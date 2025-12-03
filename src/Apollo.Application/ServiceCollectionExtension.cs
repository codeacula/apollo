using Apollo.Application.Services;
using Apollo.Core.Infrastructure.Services;

using Microsoft.Extensions.DependencyInjection;

namespace Apollo.Application;

public static class ServiceCollectionExtension
{
  public static IServiceCollection AddApplicationServices(this IServiceCollection services)
  {
    _ = services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<IApolloApplication>());

    _ = services.AddScoped<IUserValidationService, UserValidationService>();

    return services;
  }
}
