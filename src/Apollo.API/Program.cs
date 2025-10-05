using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Rest;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Hosting.Services.ComponentInteractions;
using NetCord.Services.ComponentInteractions;
using Quartz;
using Apollo.API;
using Apollo.Core.Configuration;
using Apollo.Core.Services;
using Apollo.Database;
using Apollo.Database.Services;

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
        .AddGatewayHandlers(typeof(IApolloAPIApp).Assembly)
        .AddGatewayHandlers(typeof(Apollo.Discord.IApolloDiscord).Assembly);

    var connectionString = builder.Configuration.GetConnectionString("Rydia") ?? throw new NullReferenceException();

    builder.Services.AddDbContextPool<ApolloDbContext>(options =>
    {
        options.UseNpgsql(connectionString);
    });

    // Register settings service
    builder.Services.AddScoped<ISettingsService, SettingsService>();
    
    // Register settings provider for IOptions pattern
    builder.Services.AddSingleton<ISettingsProvider, SettingsProvider>();
    builder.Services.AddSingleton<IOptions<ApolloSettings>, ApolloSettingsOptions>();

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
        var dbContext = scope.ServiceProvider.GetRequiredService<ApolloDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    // Initialize settings from database
    var settingsProvider = app.Services.GetRequiredService<ISettingsProvider>();
    await settingsProvider.ReloadAsync();

    app.AddModules(typeof(IApolloAPIApp).Assembly);
    app.AddModules(typeof(Apollo.Discord.IApolloDiscord).Assembly);
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