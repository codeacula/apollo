using Microsoft.EntityFrameworkCore;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services.ApplicationCommands;
using Quartz;
using Rydia;
using Rydia.Database;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers();

builder.Services
    .AddHttpApplicationCommands();

builder.Services
.AddDiscordGateway(options =>
{
    options.Intents = GatewayIntents.All;
})
.AddGatewayHandlers(typeof(IRydiaApp).Assembly);

var connectionString = builder.Configuration.GetConnectionString("Rydia") ?? throw new NullReferenceException();

builder.Services.AddDbContextPool<RydiaDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

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

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();

app.UseHttpsRedirection();

app.UseDefaultFiles();
app.UseStaticFiles();

await app.RunAsync();
