using Apollo.AI;
using Apollo.Application.People;
using Apollo.Application.ToDos;
using Apollo.Core.People;

using Microsoft.Extensions.DependencyInjection;

namespace Apollo.Application;

public static class ServiceCollectionExtension
{
  public static IServiceCollection AddApplicationServices(this IServiceCollection services)
  {
    _ = services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<IApolloApplication>());

    _ = services.AddScoped<IPersonService, PersonService>();
    _ = services.AddScoped<ToDoPlugin>();

    _ = services.AddTransient(_ => TimeProvider.System);

    return services;
  }

  public static IServiceProvider ConfigureAIPlugins(this IServiceProvider serviceProvider)
  {
    var aiAgent = serviceProvider.GetRequiredService<IApolloAIAgent>();
    var toDoPlugin = serviceProvider.GetRequiredService<ToDoPlugin>();
    aiAgent.AddPlugin(toDoPlugin, "ToDos");

    return serviceProvider;
  }
}
