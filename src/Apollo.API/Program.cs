using Apollo.Application.Configuration;
using Apollo.Cache;
using Apollo.Database;
using Apollo.GRPC;


WebApplicationBuilder webAppBuilder = WebApplication.CreateBuilder(args);
var configuration = webAppBuilder.Configuration;

_ = webAppBuilder.Services.AddControllers();
_ = webAppBuilder.Services
  .AddCacheServices(configuration.GetConnectionString("Redis")!)
  .AddGrpcClientServices()
  .AddDatabaseServices(configuration);

// Register MediatR scoped to only the configuration handlers.
// Apollo.API is a pure REST gateway and only needs configuration CQRS handlers.
// We register handlers explicitly to avoid pulling in AI-dependent handlers
// from the full Apollo.Application assembly scan.
_ = webAppBuilder.Services.AddMediatR(cfg =>
  cfg.RegisterServicesFromAssemblyContaining<GetInitializationStatusQueryHandler>()
);

// Disable DI validate-on-build: Apollo.API is a gateway — not all registered handlers
// (from transitive assembly scans) need resolvable dependencies in this host.
webAppBuilder.Host.UseDefaultServiceProvider(options =>
{
  options.ValidateOnBuild = false;
  options.ValidateScopes = webAppBuilder.Environment.IsDevelopment();
});

WebApplication app = webAppBuilder.Build();

_ = app.UseRequestLocalization();

if (app.Environment.IsDevelopment())
{
  _ = app.MapOpenApi();
}

_ = app.MapControllers();
_ = app.UseHttpsRedirection();
_ = app.UseDefaultFiles();
_ = app.UseStaticFiles();

await app.RunAsync();
