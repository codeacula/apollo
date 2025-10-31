using NetCord.Gateway;
using NetCord.Hosting.Gateway;

namespace Apollo.Discord.Handlers;

public class MessageCreateHandler(ILogger<MessageCreateHandler> logger) : IMessageCreateGatewayHandler
{
  public ValueTask HandleAsync(Message arg)
  {
    logger.LogInformation("{}", arg.Content);
    return default;
  }
}
