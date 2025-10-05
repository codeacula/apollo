using NetCord;
using NetCord.Rest;

namespace Apollo.Discord.Components;

public partial class ToDoRoleSelectComponent : ComponentContainerProperties
{
    public const string CustomId = "to_do_role_select";
    public ToDoRoleSelectComponent() : base()
    {
        AccentColor = Constants.Colors.ApolloGreen;
        Components = [
            new TextDisplayProperties("# Select Notification Role"),
            new TextDisplayProperties("Select which role you would like to receive notifications."),
            new RoleMenuProperties(CustomId)
        ];
    }
}