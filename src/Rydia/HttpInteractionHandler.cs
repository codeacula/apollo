using NetCord;
using NetCord.Hosting;

namespace Rydia;

public class HttpInteractionHandler(ILogger<HttpInteractionHandler> logger) : IHttpInteractionHandler
{
    public ValueTask HandleAsync(Interaction interaction)
    {
        logger.LogInformation("Received interaction: {}", interaction);
        return default;
    }
}