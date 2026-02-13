using Apollo.Cache;
using Apollo.GRPC;


WebApplicationBuilder webAppBuilder = WebApplication.CreateBuilder(args);
var configuration = webAppBuilder.Configuration;

_ = webAppBuilder.Services.AddControllers();
_ = webAppBuilder.Services
  .AddCacheServices(configuration.GetConnectionString("Redis")!)
  .AddGrpcClientServices();

WebApplication app = webAppBuilder.Build();

_ = app.UseRequestLocalization();

if (app.Environment.IsDevelopment())
{
  _ = app.MapOpenApi();
}

_ = app.UseHttpsRedirection();
_ = app.UseDefaultFiles();
_ = app.UseStaticFiles();
_ = app.MapControllers();

await app.RunAsync();
