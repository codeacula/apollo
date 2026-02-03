using Apollo.Domain.People.ValueObjects;

using NetCord.Gateway;

using ApolloPlatform = Apollo.Domain.Common.Enums.Platform;

namespace Apollo.Discord.Extensions;

public static class NetcordMessageExtension
{
  public static PlatformId GetDiscordPlatformId(this Message message)
  {
    return new PlatformId(message.Author.Username, message.Author.Id.ToString(CultureInfo.InvariantCulture), ApolloPlatform.Discord);
  }
}
