# Rydia.Discord

This library contains all Discord/NetCord-specific functionality for the Rydia bot application.

## Structure

- **Components/**: Discord UI components
  - `GeneralErrorComponent.cs`: Error display component
  - `ToDoChannelSelectComponent.cs`: Channel selection component

- **Modules/**: Discord command and interaction handlers
  - `RydiaApplicationCommands.cs`: Slash command handlers
  - `RydiaChannelMenuInteractions.cs`: Channel menu interaction handlers

- **Constants/**: Discord-specific constants
  - `Colors.cs`: NetCord color definitions for Discord embeds

## Dependencies

- NetCord and NetCord.Hosting packages for Discord integration
- Microsoft.Extensions.Logging.Abstractions for logging
- Rydia.Core for shared constants and utilities

## Usage

This library is automatically registered in the main Rydia application via Program.cs.
All Discord modules are discovered and registered through the NetCord hosting services.