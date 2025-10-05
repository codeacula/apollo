# Issue 13

## Description

```markdown
# Problem

When a user runs the `/configure-daily-alert` command, they are given a list of forum-type channels on Discord to select from for the Daily Updates. We need to be able to capture which forum is selected and store that forum as `SettingKeys.DailyAlertChannelId` in the database.

## Acceptance Criteria

1. User can initiate a `/configure-daily-alert` command in Discord and be shown the forum select list (already working)
2. The forum the user selects is stored in the Settings table, and read back to the user for confirmation
```

---

## Plan

- **Context**
  - `ApolloApplicationCommands.ConfigureDailyAlertAsync` currently defers the slash command response and surfaces `ToDoChannelSelectComponent`, but no persistence happens after a user picks a forum channel.
  - `ApolloChannelMenuInteractions.ConfigureDailyAlertAsync` handles the channel menu submission and immediately swaps in `ToDoRoleSelectComponent`; the selected channel ID is neither validated nor stored.
  - `SettingsService` (registered in `Apollo.API`) can persist key/value pairs keyed by `SettingKeys`, including `DailyAlertChannelId`, but its interface currently lives in `Apollo.Database`, so Discord code has no lightweight abstraction to depend on.

- **Implementation Steps**
  1. Relocate the `ISettingsService` interface into `Apollo.Core` (maintaining namespace clarity) so non-database projects can depend on the abstraction without referencing EF Core types; update `Apollo.Database` to implement the relocated interface and adjust DI registrations accordingly.
  2. Update `ApolloChannelMenuInteractions` to accept `ISettingsService` and any additional dependencies (e.g., `ILogger` scope helpers) via constructor injection, ensuring NetCord's module registration still succeeds.
  3. Within the channel menu handler, extract and validate the single selected forum channel ID (selector already behaves as single-choice), log the attempt, and call `SetSettingAsync(SettingKeys.DailyAlertChannelId, channelId.ToString(CultureInfo.InvariantCulture))`; surface `GeneralErrorComponent` on failure (no selection, DB error).
  4. After a successful save, update the interaction response with a confirmation component that mentions the chosen forum (e.g., `<#channelId>`) and then transition into the existing `ToDoRoleSelectComponent` to preserve the current workflow.
  5. Introduce a reusable success component aligned with Discord UI conventions for the confirmation response; add targeted logging around success/failure paths and, if feasible, add lightweight unit coverage around the interaction handler using NetCord testing utilities or dependency fakes to guard future regressions.

- **Open Questions**
  - None currently; outstanding decisions were resolved by the product owner.

- **Risks & Mitigations**
  - Relocating `ISettingsService` into `Apollo.Core` may require broad namespace updates; mitigate by introducing the interface first, updating consumers, and only then removing the old definition.
  - If the interaction handler throws before responding, the Discord UX breaks; wrap persistence in try/catch and always finalize the interaction with either confirmation or error feedback.

- **Verification**
  - Run the bot in a test Discord guild, execute `/configure-daily-alert`, select a forum channel, and confirm the response mentions the saved channel.
  - Query the `Settings` table (via EF or database client) to ensure `daily_alert_channel_id` reflects the selected forum ID.
  - Repeat the flow to confirm updates overwrite the previous value and the confirmation reflects the latest selection.

---

## Decisions

- Relocate `ISettingsService` into `Apollo.Core` so non-database projects can depend on the abstraction without inheriting EF Core concerns.
- Inject `ISettingsService` into the channel menu interaction handler to leverage existing persistence logic instead of reinventing storage within the Discord layer.
- Keep the flow moving into `ToDoRoleSelectComponent` after confirming the channel selection.
- Accept the current channel selector configuration as effectively single-choice; no extra MaxValues configuration required.
- Introduce a dedicated success confirmation component to acknowledge persisted selections.

---

## Changelog

- 2025-10-03: Authored the implementation plan for persisting the daily alert forum selection.
- 2025-10-03: Updated plan, decisions, and risks per product owner guidance on dependencies, flow, and UI.
- 2025-10-03: Relocated `ISettingsService` abstraction to `src/Apollo.Core/Services/ISettingsService.cs` and updated consuming code in `src/Apollo.Database/Services/SettingsService.cs` and `src/Apollo.API/Program.cs`.
- 2025-10-03: Persisted `/configure-daily-alert` channel selections in `src/Apollo.Discord/Modules/ApolloChannelMenuInteractions.cs` with new success feedback component (`src/Apollo.Discord/Components/SuccessNotificationComponent.cs`) and LoggerMessage-based logging.


