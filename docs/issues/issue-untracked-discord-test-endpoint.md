# Issue untracked

## Description

```markdown
Create a test API endpoint that, when reached, sends a test message to a manually-configured Discord channel.
```

---

## Plan

- Context
  - The repo uses NetCord (Gateway + Rest) and already wires Hosting integrations in `Apollo.API/Program.cs` via `.AddDiscordGateway()` and `.AddDiscordRest()`.
  - Discord-oriented code lives in `Apollo.Discord/*` and currently handles slash commands and component interactions; there is no API-layer helper for "send a message by channel id".
  - The selected channel id is persisted in DB under `ApolloSettings.Keys.DailyAlertChannelId` and exposed via `ISettingsProvider` as `ApolloSettings.DailyAlertChannelId`.

- Proposed approach (implementation plan)
  1. Create a lightweight sender abstraction in API: `IDiscordMessageSender` with a method like `Task<(bool Success, string? Error, ulong? MessageId)> SendToDailyAlertAsync(string content, CancellationToken ct)`. This enables testing and avoids coupling controller directly to NetCord.
  2. Implement `NetCordDiscordMessageSender` that uses:
     - `ISettingsProvider` to read `DailyAlertChannelId`.
     - `NetCord.Rest.RestClient` (provided by `.AddDiscordRest()`) to send a message to the channel, building `MessageProperties` with the provided content.
     - Return structured result (success, message id if available) and use `LoggerMessage` partial methods for diagnostics.
  3. Register the sender in DI in `Program.cs` (Scoped or Singleton; Scoped is fine as it uses singleton services underneath): `services.AddScoped<IDiscordMessageSender, NetCordDiscordMessageSender>();` Also remove any unnecessary usings per repo guidance.
  4. Extend `ApiController` with a new endpoint:
     - Route: `GET /api/discord/test-message`
     - Query: `content` (optional; default to `"Apollo test message from API at {UtcNow:O}"`).
     - Behavior:
       - If `DailyAlertChannelId` is not set, return `400` with a clear error payload.
       - Otherwise call `IDiscordMessageSender.SendToDailyAlertAsync` and return `200` JSON summary `{ channelId, content, messageId }` on success; if Discord call fails, return `502` with error details masked.
     - Add `LoggerMessage`-based logs for hit/start/success/failure.

- API contract (acceptance criteria)
  - Method: `GET /api/discord/test-message?content=Hello`
  - 200 OK (application/json): `{ "channelId": 123456789012345678, "content": "Hello", "messageId": 987654321098765432 }`
  - 400 BadRequest if `DailyAlertChannelId` is not configured.
  - 502 BadGateway if Discord REST call fails (network/API error). 500 for unexpected errors.

- Configuration & secrets
  - Re-use existing Discord bot configuration driven by NetCord Hosting. No new config keys are required.
  - Endpoint uses already stored `daily_alert_channel_id`. If not set, users can set it via the existing Discord flow (`/configure-daily-alert`) or by inserting the setting directly in the DB for testing.

- Tests
  - Controller unit tests:
    - Returns 400 when `DailyAlertChannelId` is null.
    - Returns 200 when sender succeeds; verifies it passes content (default and overridden).
    - Returns appropriate error when sender indicates failure.
  - Sender tests (optional if thin):
    - Validate behavior when settings missing.
    - Validate mapping of exceptions to result (could be covered by integration tests instead).

- Files to add/edit
  - New: `Apollo.API/Services/IDiscordMessageSender.cs` (interface)
  - New: `Apollo.API/Services/NetCordDiscordMessageSender.cs` (implementation using NetCord RestClient)
  - Edit: `Apollo.API/Program.cs` to register the sender
  - Edit: `Apollo.API/Controllers/ApiController.cs` to add new action method and structured responses
  - New: Tests under `tests/Apollo.API.Tests/Controllers/ApiControllerTests.cs` for success and failure paths

- Verification (manual)
  - Precondition: `daily_alert_channel_id` is set in the DB to a channel the bot can write to, and bot token is valid; application is running.
  - Trigger with PowerShell (Windows):
    - Using curl.exe
      ```powershell
      curl.exe -s "https://localhost:5001/api/discord/test-message?content=Hello%20from%20API" -k | ConvertFrom-Json | Format-List
      ```
    - Default content
      ```powershell
      curl.exe -s "https://localhost:5001/api/discord/test-message" -k | ConvertFrom-Json | Format-List
      ```
  - Observe message appears in the configured Discord channel.

- Verification (automated)
  - Run `tests/Apollo.API.Tests` after adding new tests; ensure all tests pass in CI.
  - Consider adding a resilience test that stubs sender failure to ensure 502 path.

- Notes on style/perf
  - Use `LoggerMessage` source generators for structured logs per Apollo Copilot instructions.
  - Remove unnecessary usings in new files.

---

## Decisions

- Introduce `IDiscordMessageSender` abstraction in API layer to decouple controllers from NetCord and simplify testing.
- Keep the endpoint under the existing `ApiController` with route `/api/discord/test-message` and `GET` semantics for ease of manual testing.
- Prefer `LoggerMessage` source-generated logs and remove unnecessary usings in new files.
- Return typed/structured JSON responses instead of plain strings for operability.

Open questions
- Should this endpoint be gated (e.g., development-only or header-based auth) to prevent accidental/unauthorized use? Proposal: restrict to Development by default and/or require a simple shared secret header; pending product decision.
- Response shape: do we want to include the full `RestMessage` data or only ids? Current plan: only `channelId`, `content`, and `messageId` for minimal surface.

Risks & mitigations
- Security exposure: An unauthenticated endpoint that posts to Discord can be abused.
  - Mitigation: gate by environment or simple auth header; document and follow up to harden before production.
- Misconfiguration: `daily_alert_channel_id` not set or bot lacks channel permissions.
  - Mitigation: explicit 400 with guidance; log warnings; document preconditions in README.
- Discord API/network failures.
  - Mitigation: map to 502 and include correlation id in logs; consider retries if needed (not required for test endpoint).

---

## Changelog

- 2025-10-20: Initial planning document created for an untracked endpoint to send a Discord test message from the API.
- 2025-10-20: Expanded plan with API contract, verification steps, risks, and testing strategy.
- 2025-10-20: Implemented endpoint and supporting services.
  - Added IDiscordMessageSender and NetCordDiscordMessageSender in `Apollo.API/Services` to send messages to the configured channel via NetCord RestClient.
  - Registered sender in DI within `Apollo.API/Program.cs`.
  - Extended `ApiController` with `GET /api/discord/test-message` endpoint with optional `content` query param; returns 200 with messageId on success, 400 if channel not configured, 502 on Discord failure.
  - Added unit tests under `tests/Apollo.API.Tests/Controllers/ApiControllerTests.cs` covering success, bad request, bad gateway, default content, and ping regression.
  - Used LoggerMessage source generator for logging and removed unnecessary usings in new files.
  - Added forum post capability:
    - Extended `IDiscordMessageSender` with `CreateForumPostAsync` for creating a new thread in a forum-type channel.
    - Implemented method in `NetCordDiscordMessageSender` using `RestClient.CreateForumGuildThreadAsync` with `ForumGuildThreadProperties`/`ForumGuildThreadMessageProperties`.
    - Added API endpoint `POST /api/discord/forum-post` that accepts `channelId`, `title`, `content`, and optional `tagIds` (query params). Returns `200` with `{ channelId, title, content, threadId, messageId, appliedTagIds }` on success; returns `400` when `channelId` missing; maps REST failure to `502`; `500` on exception.
    - Augmented tests in `tests/Apollo.API.Tests/Controllers/ApiControllerTests.cs` to cover forum endpoint success, bad request, failure, and defaulting behavior.
