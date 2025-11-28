using Apollo.Discord;
using Apollo.GRPC;

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

builder.Configuration.AddEnvironmentVariables()
  .AddUserSecrets<IApolloDiscord>();

// Add services to the container.
builder.Services
  .AddGrpcClientServices()
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
  .AddGatewayHandlers(typeof(IApolloDiscord).Assembly);

var app = builder.Build();

app.AddModules(typeof(IApolloDiscord).Assembly);
app.UseHttpInteractions("/interactions");
app.UseRequestLocalization();

await app.RunAsync();
