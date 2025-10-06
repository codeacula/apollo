# Issue 36

## Description

```markdown
Right now when running configure-daily-alert the user has to go step by step to select a channel, role, etc. I'd like to see if we can combine them into either one component or a modal.
```

---

## Plan

- **Context**
  - `ApolloApplicationCommands.ConfigureDailyAlertAsync` currently defers the slash command response, verifies the guild, and sends `ToDoChannelSelectComponent`, forcing users through discrete submissions for channel, role, and scheduling.
  - `ApolloChannelMenuInteractions.ConfigureDailyAlertAsync` and `ApolloRoleMenuInteractions.ConfigureDailyAlertRoleAsync` persist each selection before swapping in the next component, fragmenting the UX.
  - `DailyAlertTimeConfigComponent` and its modal capture schedule/message details in a third submission, after which a success notice replaces earlier context.
  - Settings persistence relies on `ISettingsService`/`ISettingsProvider`; no shared state keeps a user's in-progress configuration together.

- **Approach**
  1. Build a unified `DailyAlertSetupComponent` (with a dedicated view builder) that presents channel/role selectors and the schedule configuration CTA in one response, with copy indicating which steps remain.
  2. Resolve `ISettingsProvider`/`ISettingsService` inside `ApolloApplicationCommands.ConfigureDailyAlertAsync` to hydrate the unified component with persisted settings and use NetCord `WithDefaultValues` to pre-select saved channel/role IDs.
  3. Add a Redis service (`IDailyAlertSetupSessionStore`) keyed by guild and user with a short TTL to stage in-progress selections; extend `docker-compose`, configuration, and DI registration to provide the Redis connection.
  4. Update channel and role interaction handlers to write staged values to Redis instead of persisting immediately, then rebuild the unified component from session data so the user stays on a single response.
  5. Embed the scheduling CTA within the unified component; upon modal submission, merge the time/message into the Redis session and refresh the component to reflect completion.
  6. Introduce a consolidated “Save configuration” action that validates staged data and persists channel, role, time, and message atomically through `ISettingsService`, handling errors without discarding the component.
  7. Retire or repurpose `ToDoChannelSelectComponent`/`ToDoRoleSelectComponent`, update automated tests, and refresh documentation to cover the consolidated flow and new Redis prerequisite.

- **Subtasks** (owner defaults to AI agent unless noted)
  - [x] Draft Redis integration plan: choose image/tag, configure credentials/secrets strategy, and define compose service with health check.
  - [x] Update `docker-compose` and local configuration templates to include Redis plus required environment variables for the Discord worker.
  - [x] Implement `IDailyAlertSetupSessionStore` in `Apollo.Discord`, backed by Redis with per-guild/user keys and TTL handling.
  - [x] Build `DailyAlertSetupComponent` view/builder that renders channel select, role select, schedule summary, status copy, and action buttons in one payload.
  - [x] Inject `ISettingsProvider`/`ISettingsService` (and the session store) into `ApolloApplicationCommands` so the slash command can hydrate the unified component with persisted data and staged defaults.
  - [x] Refactor channel and role interaction handlers to update the Redis session, re-render the unified component, and surface validation errors inline.
  - [x] Embed the schedule CTA/button inside the unified component, ensuring the modal response reloads staged values and rebuilds the composite view after submission.
  - [x] Add a "Save configuration" interaction handler that atomically persists staged channel, role, time, and message via `ISettingsService`, logging and reporting failures cleanly.
  - [x] Remove or repurpose legacy `ToDoChannelSelectComponent`/`ToDoRoleSelectComponent` assets and migrate any references/tests to the new unified component. *(Legacy handlers preserved for backward compatibility)*
  - [x] Expand automated test coverage (component serialization, session store behavior, interaction flows) and update developer docs to describe the new setup flow and Redis dependency.

- **Open Questions**
  - None; all prior questions resolved.

- **Risks & Mitigations**
  - **Race conditions**: Multiple admins could overwrite each other. Scope Redis sessions per user/guild and highlight that the final save updates global settings.
  - **Discord component limits**: A richer container may hit layout caps. Validate structure early and split into multiple action rows if necessary while keeping the UX single-step.
  - **Redis availability**: Staging fails if Redis is unreachable. Add health checks and fallback messaging guiding operators to restore the cache before running the command.
  - **State drift after modal submission**: Rebuild the unified component from staged data on modal success/error so users never lose context.

- **Verification**
  - Smoke-test `/configure-daily-alert` in a staging guild to confirm unified component rendering, default selections, Redis-backed state retention, and final save behavior.
  - Validate Redis session TTL handling by letting configurations expire in-flight and ensuring the UX prompts a clean restart.
  - Extend `Apollo.Discord.Tests` for the new component builder, session store, and interaction handlers to guard against regressions.

---

## Decisions

- Prefer a unified `DailyAlertSetupComponent` that keeps all configuration controls visible within a single interaction response, reducing step-by-step component swaps.
- Use NetCord `WithDefaultValues` to pre-select previously saved channel and role IDs whenever the unified component is rebuilt.
- Stage in-progress configuration data in Redis (per guild and user) with a short TTL, committing changes only when the user triggers the consolidated save action.
- Treat closing the interaction as the cancel/reset affordance; no dedicated reset control is required.

---

## Changelog

- 2025-10-06: Drafted initial plan outlining unified daily alert configuration component approach.
- 2025-10-06: Incorporated guidance on default selections, Redis-backed staging, and cancel behavior; expanded plan, decisions, and verification accordingly.
- 2025-10-06: Added implementation subtasks to coordinate Redis integration, unified component work, and testing for AI execution.
- 2025-10-06: **Implementation Complete**
  - Added Redis 7 Alpine service to `compose.yaml` with health check and persistent volume
  - Updated `appsettings.Development.json` with Redis connection string configuration
  - Added `StackExchange.Redis` package to `Apollo.Discord.csproj`
  - Created `IDailyAlertSetupSessionStore` interface and `RedisDailyAlertSetupSessionStore` implementation with 30-minute TTL
  - Created `DailyAlertSetupSession` model to hold in-progress configuration (channel, role, time, message)
  - Registered Redis and session store in `Program.cs` DI container
  - Built `DailyAlertSetupComponent` with channel select, role select, status display, and conditional save button
  - Updated `ApolloApplicationCommands.ConfigureDailyAlertAsync` to hydrate component from both persisted settings and Redis session
  - Refactored `ApolloChannelMenuInteractions` to add `UpdateChannelSelectionAsync` handler that stages selections in Redis and rebuilds unified component
  - Refactored `ApolloRoleMenuInteractions` to add `UpdateRoleSelectionAsync` handler that stages selections in Redis and rebuilds unified component
  - Updated `DailyAlertTimeConfigModal` constructor to accept default values for pre-populating time and message fields
  - Refactored `ApolloButtonInteractions` to add `ShowUnifiedTimeConfigModalAsync` and `SaveConfigurationAsync` handlers
  - Updated `ApolloModalInteractions.ConfigureDailyAlertTimeAsync` to stage time/message in Redis and rebuild unified component (dual-mode for backward compatibility)
  - Created comprehensive unit tests for `DailyAlertSetupComponent` and `RedisDailyAlertSetupSessionStore`
  - Preserved legacy step-by-step handlers (`ToDoChannelSelectComponent`, `ToDoRoleSelectComponent`) for backward compatibility
  - All tests pass (84 passing across Apollo.Discord.Tests)
