using Apollo.Application.People;
using Apollo.Core.People;

using Microsoft.Extensions.DependencyInjection;

namespace Apollo.Application;

public static class ServiceCollectionExtension
{
  public static IServiceCollection AddApplicationServices(this IServiceCollection services)
  {
    _ = services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<IApolloApplication>());

    _ = services.AddScoped<IPersonService, PersonService>();

    _ = services.AddTransient(_ => TimeProvider.System);

    return services;
  }
}
