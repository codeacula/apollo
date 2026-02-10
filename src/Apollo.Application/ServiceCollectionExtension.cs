using Apollo.Application.ToDos;
using Apollo.Core.ToDos;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Apollo.Application;

public static class ServiceCollectionExtension
{
  public static IServiceCollection AddApplicationServices(this IServiceCollection services)
  {
    _ = services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<IApolloApplication>());

    services.TryAddScoped<IToDoReminderScheduler, NoOpToDoReminderScheduler>();

    _ = services.AddTransient(_ => TimeProvider.System);

    _ = services.AddSingleton<IFuzzyTimeParser, FuzzyTimeParser>();

    return services;
  }
}
