# Rydia Codebase Instructions

This document provides essential guidance for AI coding agents working in the Rydia codebase.

## Architecture Overview

Rydia is a Discord bot with a web interface, structured as:

- Backend (`src/Rydia.API/`): ASP.NET Core application that:
  - Handles Discord gateway integration using NetCord
  - Provides REST API endpoints
  - Uses Quartz for scheduled tasks with PostgreSQL persistence
  - Implements Discord slash commands via modules in `DiscordModules/`

- Frontend (`src/Client/`): Vue 3 + TypeScript application using Vite that:
  - Provides web interface for bot configuration
  - Built with modern Vue 3 composition API
  - Uses TypeScript for type safety

## Development Workflow

### Prerequisites

- Docker and Docker Compose
- .NET 9.0 SDK
- Node.js for Vue development

### Environment Setup

1. Create `.env` file in root with required variables:

   ```env
   POSTGRES_USER=rydia
   POSTGRES_PASSWORD=rydia
   POSTGRES_DB=rydia_db
   ```

2. Database initialization:
   - PostgreSQL schemas are in `src/Rydia.API/sql/`
   - Quartz tables must be initialized using `quartz_tables_postgres.sql`

### Running the Application

Development:

```powershell
# Backend
cd src/Rydia.API
dotnet run

# Frontend
cd src/Client
npm install
npm run dev
```

Docker:

```powershell
docker compose up --build
```

## Key Integration Points

1. Discord Command Registration:
   - New slash commands go in `src/Rydia.API/DiscordModules/`
   - Inherit from `ApplicationCommandModule<ApplicationCommandContext>`
   - Use `[SlashCommand]` attribute for command definitions

2. API Controllers:
   - Located in `src/Rydia.API/Controllers/`
   - Use standard ASP.NET Core controller patterns
   - Base route prefix is `/api`

3. Database Access:
   - Uses PostgreSQL for persistence
   - Quartz jobs use their own schema defined in SQL files
   - Connection string configured in `appsettings.json`

## Project Conventions

1. Module Organization:
   - Discord commands are organized into feature modules
   - Each module focuses on a specific bot functionality
   - Example: `ToDoModule.cs` handles todo-related commands

2. Configuration:
   - Environment variables via `.env` file
   - Sensitive data should never be committed to repo
   - Development settings in `appsettings.Development.json`

## Common Operations

1. Adding New Discord Commands:

   ```csharp
   public class NewModule : ApplicationCommandModule<ApplicationCommandContext>
   {
       [SlashCommand("command-name", "Command description")]
       public async Task HandleCommandAsync()
       {
           // Implementation
       }
   }
   ```

2. Frontend Development:
   - Components go in `src/Client/src/components/`
   - Use Vue 3 `<script setup>` syntax
   - Follow TypeScript type definitions
