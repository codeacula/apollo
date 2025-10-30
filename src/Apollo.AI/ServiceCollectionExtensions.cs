using Microsoft.Extensions.DependencyInjection;

namespace Apollo.AI;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddAiServices(this IServiceCollection services)
  {
    /*
    _ = services.Configure<AiConfig>(configuration.GetSection(nameof(AiConfig)));

    _ = services.AddSingleton<IAiBrain, AiBrain>(services =>
    {
        var aiConfig = services.GetRequiredService<IOptions<AiConfig>>().Value;
        var logger = services.GetRequiredService<ILogger<AiBrain>>();
        return new AiBrain(aiConfig, logger);
    })
      .AddSingleton(services => services.GetRequiredService<IOptions<AiConfig>>().Value)
      .AddTransient<IOrb, Orb>();
    */

    return services;
  }
}
