# Apollo.Discord

This library contains all Discord/NetCord-specific functionality for the Apollo bot application.

## Structure

- **Components/**: Discord UI components
    - `GeneralErrorComponent.cs`: Error display component
    - `SuccessNotificationComponent.cs`: Success notification component
    - `ToDoChannelSelectComponent.cs`: Channel selection component
    - `ToDoRoleSelectComponent.cs`: Role selection component
    - `DailyAlertTimeConfigComponent.cs`: Button component to trigger time/message configuration

- **Modules/**: Discord command and interaction handlers
    - `ApolloApplicationCommands.cs`: Slash command handlers
    - `ApolloChannelMenuInteractions.cs`: Channel menu interaction handlers
    - `ApolloRoleMenuInteractions.cs`: Role menu interaction handlers
    - `ApolloButtonInteractions.cs`: Button interaction handlers
    - `ApolloModalInteractions.cs`: Modal interaction handlers

- **Constants/**: Discord-specific constants
    - `Colors.cs`: NetCord color definitions for Discord embeds

## Dependencies

- NetCord and NetCord.Hosting packages for Discord integration
- Microsoft.Extensions.Logging.Abstractions for logging
- Apollo.Core for shared constants and utilities

## Usage

This library is automatically registered in the main Apollo application via Program.cs.
All Discord modules are discovered and registered through the NetCord hosting services.
