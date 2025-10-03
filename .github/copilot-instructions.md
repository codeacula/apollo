# Rydia Copilot Instructions

## Architecture
- Multi-project .NET 9 solution (`Rydia.sln`) with API (`src/Rydia.API`), shared domain (`src/Rydia.Core`), EF data layer (`src/Rydia.Database`), Discord integration (`src/Rydia.Discord`), and a Vue/Vite client (`src/Client`).
- `Program.cs` wires ASP.NET Core + NetCord hosting, loads modules from assemblies tagged with `IRydiaApp` / `IRydiaDiscord`, serves REST controllers, static files, and Quartz jobs.
- Discord workflows live under `Rydia.Discord/Modules`, built on NetCord `ApplicationCommandModule` / `ComponentInteractionModule`; UI pieces are in `Rydia.Discord/Components`.
- Shared configuration keys and constants live in `Rydia.Core`; respect them when adding new settings or Discord styling.

## Local Workflows
- Restore/build backend via `dotnet restore` then `dotnet build Rydia.sln`; run locally with `dotnet run --project src/Rydia.API`.
- Install client deps in `src/Client` with `npm install`; develop via `npm run dev` (proxies `/api` and `/interactions` to `http://localhost:5144`).
- Production bundle goes to `src/Rydia.API/wwwroot` (`npm run build` honors `vite.config.ts`), letting the API serve SPA assets.
- To run the full stack with Postgres, populate `.env` (matches keys in `compose.yaml`) and execute `docker compose up --build` from the repo root.

## Database
- PostgreSQL connection string lives in `src/Rydia.API/appsettings.Development.json` under `ConnectionStrings.Rydia`; align it with local or compose credentials.
- EF Core migrations reside in `src/Rydia.Database/Migrations`; `Database.MigrateAsync()` runs on startup, so keep migrations current before booting.
- Quartz.NET is configured for the same Postgres instance using `QRTZ_` tables—ensure the Quartz schema migration is applied before scheduling jobs.
- `RydiaDbContext` enforces unique setting keys; extend `OnModelCreating` if additional entities need constraints.

## Settings Service
- `SettingsService` is the gateway for configuration values; only keys listed in `Rydia.Core/Constants/SettingKeys.cs` pass validation.
- Use the typed helpers (`GetBooleanSettingAsync`, `SetBooleanSettingAsync`, etc.) to avoid manual parsing and keep logging consistent.
- Updates replace records via remove/add; wrap multi-setting updates in a transaction if you introduce batch operations.
- Adding a new setting requires updating `SettingKeys`, seeding defaults (if needed), and updating any Discord or Quartz consumers.

## Discord Integrations
- Slash commands are defined in `Rydia.Discord/Modules/RydiaApplicationCommands.cs`; commands defer responses first, then reuse `RespondAsync` to swap in component payloads.
- Component workflows stay in matching modules (e.g., `RydiaChannelMenuInteractions.cs`) and reference component custom IDs like `ToDoChannelSelectComponent.CustomId`—keep IDs unique.
- Embed visuals centralize in `Rydia.Discord/Constants/Colors.cs` and component constructors; avoid hardcoding color literals elsewhere.
- Gateway hosts are configured with `GatewayIntents.All`; prune intents if you create specialized bots to reduce Discord load.

## Testing & Validation
- No test projects yet; when adding coverage, run `dotnet test` at the solution root so migrations/settings logic load once per suite.
- Validate the client build with `npm run build` before publishing so the Docker multi-stage copy (`Rydia.API/Dockerfile`) succeeds.
- For integration scenarios, spin up Postgres via `docker compose up pgsql` and run the API directly against it to mirror production state.
- Watch for logging via `LoggerMessage` partials—define new log methods alongside their usage to keep structured logging consistent.

## References
- Startup pipeline and service registration: `src/Rydia.API/Program.cs`
- Docker image + SPA bundling: `src/Rydia.API/Dockerfile`
- Database context, factory, and migrations: `src/Rydia.Database/`
- Discord modules and components: `src/Rydia.Discord/`
- Vue entrypoint and config: `src/Client/`

