using NetCord;
using NetCord.Rest;

namespace Rydia.Discord.Components;

public partial class ToDoChannelSelectComponent : ComponentContainerProperties
{
    public const string CustomId = "to_do_channel_select";
    public ToDoChannelSelectComponent() : base()
    {
        AccentColor = Constants.Colors.RydiaGreen;
        Components = [
            new TextDisplayProperties("# Select Forum Channel"),
            new TextDisplayProperties("Select which forum channel you would like daily updates to be posted in."),
            new ChannelMenuProperties(CustomId)
            {
                ChannelTypes = [ChannelType.ForumGuildChannel]
            }
        ];
    }
}