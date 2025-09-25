using NetCord;
using NetCord.Rest;

namespace Rydia.Discord.Components;

public partial class GeneralErrorComponent : ComponentContainerProperties
{
    public GeneralErrorComponent(string errorMessage) : base()
    {
        AccentColor = Constants.Colors.Error;
        Components = [
            new TextDisplayProperties("# Error"),
            new TextDisplayProperties(errorMessage)
        ];
    }
}