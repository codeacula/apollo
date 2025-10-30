using NetCord.Rest;

namespace Apollo.Discord.Components;

public class GeneralErrorComponent : ComponentContainerProperties
{
  public GeneralErrorComponent(string errorMessage)
  {
    AccentColor = Constants.Colors.Error;
    Components = [
        new TextDisplayProperties("# Error"),
            new TextDisplayProperties(errorMessage)
    ];
  }
}
