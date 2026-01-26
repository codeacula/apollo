using Apollo.Core.Notifications;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Apollo.Notifications;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddNotifications(this IServiceCollection services)
  {
    services.TryAddScoped<IPersonNotificationClient, NoOpPersonNotificationClient>();
    return services;
  }

  public static IServiceCollection AddNotificationsWithChannels(this IServiceCollection services)
  {
    _ = services.AddScoped<INotificationChannel, DiscordNotificationChannel>();
    _ = services.AddScoped<IPersonNotificationClient, PersonNotificationClient>();
    return services;
  }
}
