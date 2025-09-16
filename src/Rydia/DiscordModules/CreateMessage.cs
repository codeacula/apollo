using NetCord.Rest;

namespace Rydia.DiscordModules;

static T CreateMessage<T>() where T : IMessageProperties, new()
{
    return new()
    {
        Content = "",
        Components = []
    };
}