using Apollo.Discord;

using Microsoft.AspNetCore.Builder;

using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.AspNetCore;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Rest;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Hosting.Services.ComponentInteractions;
using NetCord.Services.ComponentInteractions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
  //.AddGrpcClientServices()
  .AddDiscordGateway(options =>
    {
      options.Intents = GatewayIntents.GuildMessages
                        | GatewayIntents.DirectMessages
                        | GatewayIntents.MessageContent
                        | GatewayIntents.DirectMessageReactions
                        | GatewayIntents.GuildMessageReactions;
    })
  .AddDiscordRest()
  .AddApplicationCommands()
  .AddComponentInteractions<ButtonInteraction, ButtonInteractionContext>()
  .AddComponentInteractions<StringMenuInteraction, StringMenuInteractionContext>()
  .AddComponentInteractions<UserMenuInteraction, UserMenuInteractionContext>()
  .AddComponentInteractions<RoleMenuInteraction, RoleMenuInteractionContext>()
  .AddComponentInteractions<MentionableMenuInteraction, MentionableMenuInteractionContext>()
  .AddComponentInteractions<ChannelMenuInteraction, ChannelMenuInteractionContext>()
  .AddComponentInteractions<ModalInteraction, ModalInteractionContext>()
  .AddGatewayEventHandlers(typeof(IApolloDiscord).Assembly);

var host = builder.Build();

host.AddModules(typeof(IApolloDiscord).Assembly);
host.UseGatewayEventHandlers();
host.UseHttpInteractions("/interactions");
host.UseRequestLocalization();

await host.RunAsync();
