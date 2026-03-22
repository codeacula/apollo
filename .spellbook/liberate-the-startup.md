# Liberate the Startup

**Spell ID:** 20260321131109  
**Status:** cast (11/11 units verified, 0/11 blocked)

## Request

Remove the dependency of needing a .env file to start Apollo. Refactor it so it handles starting up without immediately connecting to the AI server, and only wait until the information is provided. Create a smooth first-time run process that allows the user to configure the application and store the settings in the database. Update the Client (Vue SPA) to support initialization, designating the superadmin, etc.

## Research

### Current .env Dependencies (What Must Change)

| Variable | Used By | Current Behavior |
|----------|---------|-----------------|
| `ConnectionStrings__Apollo` | Apollo.Service (Marten + EF Core) | HARD CRASH - `MissingDatabaseStringException` |
| `ConnectionStrings__Redis` | Apollo.Service, API, Discord | HARD CRASH - null-forgiving `!` or explicit `throw` |
| `ConnectionStrings__Quartz` | Apollo.Service | HARD CRASH - `MissingDatabaseStringException` |
| `GrpcHostConfig__*` (Host/Port/ApiToken) | Apollo.API, Discord | HARD CRASH - `required` properties fail |
| `ApolloAIConfig__*` (ModelId/Endpoint/ApiKey) | Apollo.Service | SOFT FAIL - logs warning, uses empty defaults |
| `Discord__Token` / `Discord__PublicKey` | Apollo.Discord | Read by NetCord automatically |
| `SuperAdminConfig__DiscordUserId` | Apollo.Service | No crash - nullable |
| `PersonConfig__*` | Apollo.Service | No crash - has defaults |

### Decision: What Stays in .env vs. Moves to DB

**Stays in .env/appsettings (infrastructure):**
- `ConnectionStrings__Apollo` (PostgreSQL)
- `ConnectionStrings__Redis`
- `ConnectionStrings__Quartz`
- `GrpcHostConfig__*` (inter-service routing)
- `Kestrel` settings

**Moves to database (application config):**
- `ApolloAIConfig` (ModelId, Endpoint, ApiKey)
- `Discord__Token`, `Discord__PublicKey`
- `SuperAdminConfig__DiscordUserId`
- `PersonConfig` (DefaultTimeZoneId, DefaultDailyTaskCount)
- `DiscordConfig__BotName`

### No Backward Compatibility Required
- Clean break: .env-only settings are removed, DB is the source of truth for application config.

### Existing Patterns to Follow

**Marten Aggregates:** `sealed record` with `Create(IEvent<T>)` / `Apply(IEvent<T>, Aggregate)` static methods. Events registered in `ServiceCollectionExtensions`. Inline snapshot projections.

**Stores:** Primary constructor DI of `IDocumentSession`, `TimeProvider`. Return `Result<T>` from FluentResults. Try/catch with `Result.Fail(ex.Message)`.

**MediatR:** Co-located command+handler in same file. `sealed record` for command, `sealed class` with primary constructor for handler. Returns `Result<T>`.

**gRPC:** Code-first via protobuf-net.Grpc. `[DataContract]`/`[DataMember(Order=N)]`. `GrpcResult<T>` wrapper. `IAuthenticatedRequest` for auth-required calls.

**SuperAdminConfig Flow:** Currently bound from IConfiguration section, registered as singleton. Checked in PersonStore (auto-grant on create), ApolloGrpcService (prevent revoking own access), AuthorizationInterceptor (enforce [RequireSuperAdmin]). MUST be replaced with DB-backed lookup.

**Test Conventions:** xUnit 2.9.3 + Moq 4.20.72. `Assert.*` assertions. Method naming: `MethodName` + `Scenario` + `ExpectedResult` + `Async`. One test class per file. Constructor field mocks for complex tests, local mocks for simple. AAA pattern.

**Client:** Bare Vite+Vue3+TypeScript scaffold. No router, no Pinia, no API client, no tests. Build output goes to `src/Apollo.API/wwwroot`. Dev proxy: `/api` → `localhost:5144`.

### Graceful Degradation Model
- Apollo.Service starts fully even without AI, Discord, or SuperAdmin config
- Health/status endpoint reports which subsystems are configured
- AI-dependent requests return a clear "AI not configured" error instead of crashing
- A setup wizard in the Client guides first-time configuration

### Superadmin Designation
- During first-time setup, user either enters a Discord user ID manually or authenticates via Discord OAuth2 (`identify` scope → `GET /users/@me`)
- SuperAdmin is stored in the DB config aggregate

### Client Infrastructure Needed
- vue-router for navigation + setup redirect guard
- Pinia for state management
- Typed API client for `/api/*` endpoints
- Vitest + @vue/test-utils for testing
- Multi-step setup wizard component

## Acceptance Tests

- [ ] Apollo.Service starts without `.env` file; gracefully logs missing application config
- [ ] Apollo.API and Apollo.Discord start with minimal `.env` (only infrastructure strings: PostgreSQL, Redis, Quartz, gRPC host/port/token)
- [ ] AI requests return "AI not configured" error if AI credentials missing, not hard crash
- [ ] Discord bot requests return "Discord not configured" error if Discord config missing, not hard crash
- [ ] SuperAdmin check uses DB config, not IConfiguration singleton
- [ ] Application health/status endpoint reports configured subsystems
- [ ] Configuration aggregate stored in Marten with singleton root ID
- [ ] GET `/api/configuration` returns current config (or partial/empty if unconfigured)
- [ ] POST `/api/configuration/ai` updates AI config (ModelId, Endpoint, ApiKey) and persists
- [ ] POST `/api/configuration/discord` updates Discord config (Token, PublicKey, BotName) and persists
- [ ] POST `/api/configuration/superadmin` designates superadmin (Discord user ID or OAuth2 identity)
- [ ] GET `/api/configuration/status` reports which subsystems are ready
- [ ] Client detects uninitialized state and redirects to setup wizard
- [ ] Setup wizard guides through: AI config → Discord config → SuperAdmin designation
- [ ] Configuration persists across service restart
- [ ] Existing .env-based settings can be manually migrated into DB via setup wizard

## Units of Work

### 1. Configuration Aggregate & Event Model (Database Layer)
**Status:** verified  
**Description:** Create `DbConfiguration` aggregate with events for AI, Discord, and SuperAdmin settings. Use singleton root ID for application-wide config.  
**Acceptance Tests:**
- `DbConfiguration` has fields: `AiModelId`, `AiEndpoint`, `AiApiKey`, `DiscordToken`, `DiscordPublicKey`, `DiscordBotName`, `SuperAdminDiscordUserId`, `DefaultTimeZoneId`, `DefaultDailyTaskCount`
- Events defined: `AiConfigurationUpdatedEvent`, `DiscordConfigurationUpdatedEvent`, `SuperAdminConfigurationUpdatedEvent`
- Aggregate applies events via `Apply()` method
- Inline snapshot projection creates `mt_doc_dbconfiguration` table
- Singleton root ID accessible via `ConfigurationId.Root`

**Files:**
- `src/Apollo.Database/Configuration/Events/AiConfigurationUpdatedEvent.cs`
- `src/Apollo.Database/Configuration/Events/DiscordConfigurationUpdatedEvent.cs`
- `src/Apollo.Database/Configuration/Events/SuperAdminConfigurationUpdatedEvent.cs`
- `src/Apollo.Database/Configuration/DbConfiguration.cs`
- `src/Apollo.Database/ServiceCollectionExtensions.cs` (register events)

**Additional Information:**
- Use `sealed record` for events
- Use `sealed record` for aggregate
- Include `ConfigurationId` value object
- Configure inline snapshot lifecycle

---

### 2. Configuration Store (Data Access Layer)
**Status:** verified  
**Description:** Implement `IConfigurationStore` interface in Apollo.Database with CRUD operations for configuration.  
**Acceptance Tests:**
- `GetAsync()` returns current config or empty default if not initialized
- `UpdateAiAsync(modelId, endpoint, apiKey)` returns `Result<DbConfiguration>`
- `UpdateDiscordAsync(token, publicKey, botName)` returns `Result<DbConfiguration>`
- `UpdateSuperAdminAsync(discordUserId)` returns `Result<DbConfiguration>`
- All methods use primary constructor DI: `IDocumentSession`, `TimeProvider`
- Failed operations return `Result.Fail()` with meaningful message

**Files:**
- `src/Apollo.Database/Configuration/IConfigurationStore.cs`
- `src/Apollo.Database/Configuration/ConfigurationStore.cs`

**Additional Information:**
- Use `documentSession.StartStream()` for first insert
- Use `documentSession.Append()` for updates
- Follow store pattern from existing codebase (PersonStore, ToDoStore)

---

### 3. CQRS Commands & Queries (Application Layer)
**Status:** verified  
**Description:** Implement MediatR handlers for configuration queries and commands.  
**Acceptance Tests:**
- `GetConfigurationQuery` handler returns current `DbConfiguration` or empty default
- `UpdateAiConfigurationCommand` handler validates inputs and persists via store
- `UpdateDiscordConfigurationCommand` handler validates inputs and persists
- `UpdateSuperAdminConfigurationCommand` handler validates and persists
- All handlers return `Result<DbConfiguration>` or similar
- Queries and handlers are co-located in same files (e.g., `GetConfigurationQuery.cs`)

**Files:**
- `src/Apollo.Application/Configuration/GetConfigurationQuery.cs`
- `src/Apollo.Application/Configuration/UpdateAiConfigurationCommand.cs`
- `src/Apollo.Application/Configuration/UpdateDiscordConfigurationCommand.cs`
- `src/Apollo.Application/Configuration/UpdateSuperAdminConfigurationCommand.cs`

**Additional Information:**
- Commands: `sealed record` with required properties
- Handlers: `sealed class` with primary constructor
- Validate before persisting (e.g., non-empty strings for API keys)
- Return `Result<DbConfiguration>` for consistency

---

### 4. gRPC Configuration Service (Transport Layer)
**Status:** verified  
**Description:** Create `IConfigurationService` gRPC contract and implement in Apollo.Service. Expose endpoints for Client to read/update config.  
**Acceptance Tests:**
- `GetConfiguration()` returns current config or partial/empty if unconfigured
- `UpdateAiConfiguration(request)` persists AI settings and returns updated config
- `UpdateDiscordConfiguration(request)` persists Discord settings
- `UpdateSuperAdminConfiguration(request)` persists SuperAdmin
- `GetConfigurationStatus()` returns subsystem readiness flags (AI configured, Discord configured, SuperAdmin designated)
- All endpoints use protobuf-net.Grpc code-first contracts
- Results wrapped in `GrpcResult<T>`

**Files:**
- `src/Apollo.GRPC/Configuration/IConfigurationService.cs` (service contract)
- `src/Apollo.GRPC/Configuration/GetConfigurationRequest.cs` (if needed)
- `src/Apollo.GRPC/Configuration/UpdateAiConfigurationRequest.cs`
- `src/Apollo.GRPC/Configuration/UpdateDiscordConfigurationRequest.cs`
- `src/Apollo.GRPC/Configuration/UpdateSuperAdminConfigurationRequest.cs`
- `src/Apollo.GRPC/Configuration/ConfigurationResponse.cs`
- `src/Apollo.GRPC/Configuration/ConfigurationStatusResponse.cs`
- `src/Apollo.Service/GrpcServices/ConfigurationService.cs` (implementation)
- `src/Apollo.Service/Program.cs` (register service)

**Additional Information:**
- Use `[DataContract]` and `[DataMember(Order=N)]` on all DTOs
- For read operations, do NOT require auth; for writes, require `[RequiresAuthentication]` or `[RequiresSuperAdmin]` as appropriate
- Wrap all results in `GrpcResult<T>` for consistency
- Consider whether update operations should require SuperAdmin role

---

### 5. Graceful Degradation: AI Service
**Status:** verified  
**Description:** Refactor `IApolloAIAgent` initialization and error handling to gracefully degrade when AI credentials are missing.  
**Acceptance Tests:**
- `IApolloAIAgent` services register successfully even if no credentials in config
- AI-dependent operations (e.g., `ProcessIncomingMessageCommandHandler`) check config status and return clear "AI not configured" error
- Logs distinguish "AI not configured" (first-time) from "AI connection failed" (transient error)
- Health check endpoint reports AI subsystem as unconfigured if credentials missing

**Files:**
- `src/Apollo.Service/Program.cs` (refactor DI registration)
- `src/Apollo.Application/Conversations/ProcessIncomingMessageCommandHandler.cs` (add config check)
- `src/Apollo.Service/Health/HealthCheckService.cs` (report subsystem status)
- `src/Apollo.AI/ApolloAIAgent.cs` (optional: lazy initialization if beneficial)

**Additional Information:**
- Query configuration store before attempting AI operations
- Return `Result.Fail("AI not configured")` instead of throwing
- Add logging: `LogMessage.AINotConfigured()`
- Consider adding `CircuitBreakerPolicy` for transient AI failures

---

### 6. SuperAdmin & Authorization: Move to DB Config
**Status:** verified  
**Description:** Refactor SuperAdmin handling to read from DB config instead of IConfiguration singleton.  
**Acceptance Tests:**
- `PersonStore.GetOrCreateAsync()` auto-grants SuperAdmin if `person.DiscordUserId == configSuperAdminId`
- `ApolloGrpcService.RevokeSuper Admin()` checks DB config, not IConfiguration
- `AuthorizationInterceptor` checks DB config when enforcing `[RequiresSuperAdmin]` attribute
- SuperAdmin checks are consistently placed (query ConfigurationStore)

**Files:**
- `src/Apollo.Database/People/PersonStore.cs` (update to check config store)
- `src/Apollo.Service/GrpcServices/ApolloGrpcService.cs` (update SuperAdmin revocation)
- `src/Apollo.GRPC/Interceptors/AuthorizationInterceptor.cs` (update SuperAdmin check)
- Remove: `SuperAdminConfig` IConfiguration section binding

**Additional Information:**
- Inject `IConfigurationStore` into affected classes
- Cache config in memory with appropriate TTL if needed
- Ensure no breaking changes to existing SuperAdmin checks

---

### 7. Client: Vue Router & Infrastructure Setup
**Status:** verified  
**Description:** Add vue-router and Pinia for navigation and state management. Set up dev proxy to `/api` endpoint.  
**Acceptance Tests:**
- Vue Router configured with `/setup` and `/dashboard` routes
- Pinia store created for initialization state
- Dev server proxy routes `/api/*` to `http://localhost:5144`
- `src/main.ts` initializes router + pinia + mounts app

**Files:**
- `src/Client/src/router/index.ts`
- `src/Client/src/stores/setupStore.ts` (Pinia store)
- `src/Client/vite.config.ts` (add server proxy)
- `src/Client/src/main.ts` (update app initialization)
- `src/Client/src/App.vue` (update to use router-view)

**Additional Information:**
- Install dependencies: `vue-router@4`, `pinia@2`
- Add guard to redirect to `/setup` if config incomplete
- Persist setup state in Pinia for UX

**Blocker Resolution:** ✅ UNBLOCKED
- SetupWizard.test.ts immutable test stubs resolved by UoW 8 implementation
- Router/store/setup-wizard client tests now green

---

### 8. Client: Setup Wizard Component
**Status:** verified  
**Description:** Create multi-step setup wizard for configuring AI, Discord, and SuperAdmin. This unit is being used to unblock UoW 7's blocked client test stubs that require the SetupWizard component implementation.  
**Acceptance Tests:**
- Wizard displays step indicators (1/3, 2/3, 3/3)
- Step 1: AI Configuration (ModelId, Endpoint, ApiKey inputs with validation)
- Step 2: Discord Configuration (Token, PublicKey, BotName)
- Step 3: SuperAdmin Designation (manual Discord user ID input or OAuth2 flow)
- Next/Back buttons between steps
- Submit saves config to backend via gRPC
- Success message and redirect to dashboard

**Files:**
- `src/Client/src/components/SetupWizard.vue`
- `src/Client/src/views/SetupPage.vue`
- `src/Client/src/services/configApi.ts` (typed API client for `/api/configuration/*`)
- `src/Client/src/services/authService.ts` (Discord OAuth2 helper if needed)

**Additional Information:**
- Use form validation library (e.g., vee-validate)
- Show loading spinner during API calls
- Display error messages from backend
- Optional: OAuth2 flow for SuperAdmin (requires Discord app setup)
- ✅ VERIFIED: SetupWizard component implemented; router injection via componentInstance.proxy capture in beforeCreate mixin

---

### 9. Client: Configuration Status & Health Check
**Status:** verified  
**Description:** Display setup status and health of configured subsystems in dashboard.  
**Acceptance Tests:**
- Dashboard shows green checkmark for configured subsystems
- Red indicator for missing config
- "Not configured" message with link to re-run setup wizard
- Real-time status updates (optional polling or websocket)

**Files:**
- `src/Client/src/components/ConfigurationStatus.vue`
- `src/Client/src/services/healthApi.ts` (call `/api/configuration/status`)

**Additional Information:**
- Display AI status, Discord status, SuperAdmin designation
- Offer "Reconfigure" button to return to wizard
- Consider adding health check polling

---

### 10. Refactor: Remove Hard Dependencies from Program.cs
**Status:** verified  
**Description:** Update Apollo.Service, Apollo.API, and Apollo.Discord `Program.cs` to gracefully handle missing application config.  
**Acceptance Tests:**
- Apollo.Service starts without AI, Discord, or SuperAdmin config
- Services log "Subsystem not configured" as warnings, not errors
- Health check reports partial readiness (healthy but not fully configured)
- Environment variables for infrastructure (DB, Redis, gRPC) are still required

**Files:**
- `src/Apollo.Service/Program.cs`
- `src/Apollo.API/Program.cs`
- `src/Apollo.Discord/Program.cs`
- `src/Apollo.Service/Health/HealthCheckService.cs`

**Additional Information:**
- Move AI, Discord, SuperAdmin registration into conditional blocks
- Add guards in handlers to check subsystem readiness
- Update startup logging to show what's configured vs. missing

---

### 11. Integration Tests & Docs
**Status:** verified  
**Description:** Add integration tests for configuration flow and document the setup process.  
**Acceptance Tests:**
- Integration test: Get empty config → Update AI → Verify persisted → Restart service → Config still there
- Integration test: gRPC endpoints return correct responses
- Documentation covers: operator setup, developer configuration, troubleshooting
- Example `.env` includes only infrastructure vars

**Files:**
- `src/Apollo.Database.Tests/Configuration/ConfigurationStoreTests.cs`
- `src/Apollo.Application.Tests/Configuration/*HandlerTests.cs`
- `src/Apollo.Service.Tests/Configuration/ConfigurationServiceTests.cs`
- `docs/INITIALIZATION.md`
- `.env.example` (update to remove app config vars)

**Additional Information:**
- Test both success and failure paths
- Include example commands for manual testing
- Document migration guide for existing deployments

