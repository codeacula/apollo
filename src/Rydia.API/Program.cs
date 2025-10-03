using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetCord;

using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Rest;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Hosting.Services.ComponentInteractions;
using NetCord.Services.ComponentInteractions;
using Quartz;
using Rydia.API;
using Rydia.Database;
using Rydia.Database.Services;

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
        .AddGatewayHandlers(typeof(IRydiaApp).Assembly)
        .AddGatewayHandlers(typeof(Rydia.Discord.IRydiaDiscord).Assembly);

    var connectionString = builder.Configuration.GetConnectionString("Rydia") ?? throw new NullReferenceException();

    builder.Services.AddDbContextPool<RydiaDbContext>(options =>
    {
        options.UseNpgsql(connectionString);
    });

    // Register settings service
    builder.Services.AddScoped<ISettingsService, SettingsService>();

    builder.Services
        .AddQuartz(q =>
        {
            q.UsePersistentStore(s =>
            {
                s.UseProperties = true;
                s.UsePostgres(options =>
                {
                    options.ConnectionString = connectionString;
                    options.TablePrefix = "QRTZ_";
                });
                s.UseSystemTextJsonSerializer();
            });
        })
        .AddQuartzHostedService(opt =>
        {
            opt.WaitForJobsToComplete = true;
        });

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<RydiaDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    app.AddModules(typeof(IRydiaApp).Assembly);
    app.AddModules(typeof(Rydia.Discord.IRydiaDiscord).Assembly);
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