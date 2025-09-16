using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Rest;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Hosting.Services.ComponentInteractions;
using NetCord.Services.ComponentInteractions;
using Quartz;
using Rydia;

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services
        .AddControllers();

    builder.Services
    .AddDiscordGateway(options =>
    {
        options.Intents = GatewayIntents.All;
    })
        .AddApplicationCommands()
        .AddDiscordRest()
        .AddComponentInteractions<ButtonInteraction, ButtonInteractionContext>()
        .AddComponentInteractions<StringMenuInteraction, StringMenuInteractionContext>()
        .AddComponentInteractions<UserMenuInteraction, UserMenuInteractionContext>()
        .AddComponentInteractions<RoleMenuInteraction, RoleMenuInteractionContext>()
        .AddComponentInteractions<MentionableMenuInteraction, MentionableMenuInteractionContext>()
        .AddComponentInteractions<ChannelMenuInteraction, ChannelMenuInteractionContext>()
        .AddComponentInteractions<ModalInteraction, ModalInteractionContext>()
        .AddGatewayHandlers(typeof(IRydiaApp).Assembly);

    var connectionString = builder.Configuration.GetConnectionString("Rydia") ?? throw new NullReferenceException();

    builder.Services
        .AddQuartz(q =>
        {
            q.UsePersistentStore(s =>
            {
                s.UseProperties = true;
                s.UsePostgres(connectionString);
                s.UseSystemTextJsonSerializer();
            });
        })
        .AddQuartzHostedService(opt =>
        {
            opt.WaitForJobsToComplete = true;
        });

    var app = builder.Build();

    app.AddModules(typeof(IRydiaApp).Assembly);
    app.UseRequestLocalization();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.MapControllers();

    app.UseHttpsRedirection();

    app.UseDefaultFiles();
    app.UseStaticFiles();

    await app.RunAsync();
}
catch (Exception ex)
{
    await Console.Error.WriteLineAsync(ex.ToString());
}