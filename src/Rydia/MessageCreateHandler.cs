namespace Rydia;

using NetCord.Gateway;
using NetCord.Hosting.Gateway;

public class MessageCreateHandler(ILogger<MessageCreateHandler> logger) : IMessageCreateGatewayHandler
{
    public ValueTask HandleAsync(Message message)
    {
        logger.LogInformation("{}", message.Content);
        return default;
    }
}