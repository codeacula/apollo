using Microsoft.Extensions.DependencyInjection;

namespace Apollo.Application;

public static class ServiceCollectionExtension
{
  public static IServiceCollection AddApplicationServices(this IServiceCollection services)
  {
    _ = services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<IApolloApplication>());

    return services;
  }
}
