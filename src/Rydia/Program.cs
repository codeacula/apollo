using NetCord.Gateway;
using NetCord.Hosting.AspNetCore;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Rest;
using NetCord.Hosting.Services.ApplicationCommands;
using Rydia;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers();

builder.Services.AddDiscordRest()
    .AddHttpApplicationCommands();

builder.Services
.AddDiscordGateway(options =>
{
    options.Intents = GatewayIntents.All;
})
.AddGatewayHandlers(typeof(IRydiaApp).Assembly);

var app = builder.Build();

// Configure the HTTP request pipeline. 
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseHttpInteractions("/interactions");

app.MapFallbackToFile("index.html");

app.Run();
