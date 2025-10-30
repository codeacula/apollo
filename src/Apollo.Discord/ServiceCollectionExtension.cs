using Apollo.Discord.Services;

using Microsoft.Extensions.DependencyInjection;

using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Rest;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Hosting.Services.ComponentInteractions;
using NetCord.Services.ComponentInteractions;

namespace Apollo.Discord;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddDiscordServices(this IServiceCollection services)
  {
    _ = services
    .AddDiscordGateway(options => options.Intents = GatewayIntents.All)
        .AddApplicationCommands()
        .AddDiscordRest()
        .AddComponentInteractions<ButtonInteraction, ButtonInteractionContext>()
        .AddComponentInteractions<StringMenuInteraction, StringMenuInteractionContext>()
        .AddComponentInteractions<UserMenuInteraction, UserMenuInteractionContext>()
        .AddComponentInteractions<RoleMenuInteraction, RoleMenuInteractionContext>()
        .AddComponentInteractions<MentionableMenuInteraction, MentionableMenuInteractionContext>()
        .AddComponentInteractions<ChannelMenuInteraction, ChannelMenuInteractionContext>()
        .AddComponentInteractions<ModalInteraction, ModalInteractionContext>()
        .AddGatewayHandlers(typeof(IApolloDiscord).Assembly);

    _ = services.AddScoped<IDiscordMessageSender, NetCordDiscordMessageSender>();

    return services;
  }
}
