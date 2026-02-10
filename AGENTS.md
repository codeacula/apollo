# Repository Guidelines

See `ARCHITECTURE.md` for comprehensive documentation on architecture, coding practices, NuGet packages, and local development setup.

## Project Structure & Module Organization

- `src/` holds all runtime code:
  - **Apollo.Service** - Main backend host: gRPC server, Quartz scheduler, database, cache, AI, notifications
  - **Apollo.API** - HTTP/REST gateway with OpenAPI; communicates with Service via gRPC
  - **Apollo.Discord** - Discord bot host using NetCord; communicates with Service via gRPC
  - **Apollo.Application** - CQRS use-cases with MediatR commands/queries/handlers
  - **Apollo.Domain** - Core entities, value objects, domain services (Conversations, People, ToDos)
  - **Apollo.Core** - Shared contracts, DTOs, logging utilities, abstractions
  - **Apollo.AI** - Semantic Kernel AI agents, plugins, prompt management
  - **Apollo.Database** - Marten event sourcing + EF Core data access
  - **Apollo.GRPC** - Shared gRPC contracts, clients, and interceptors (library, not a host)
  - **Apollo.Cache** - Redis caching infrastructure
  - **Apollo.Notifications** - Notification channel abstractions and implementations (Discord DMs)
  - **Client** - Vue 3 + Vite + TypeScript SPA frontend
- `tests/` mirrors runtime projects with xUnit projects named `*.Tests`.
- `assets/` stores shared static files.
- Solution entry point is `Apollo.sln`; environment templates live in `.env.example`.

## Build, Test, and Development Commands

- Restore/build: `dotnet restore Apollo.sln && dotnet build Apollo.sln`
- Run Service (backend): `dotnet run --project src/Apollo.Service/Apollo.Service.csproj`
- Run API (HTTP gateway): `dotnet run --project src/Apollo.API/Apollo.API.csproj`
- Run Discord bot: `dotnet run --project src/Apollo.Discord/Apollo.Discord.csproj`
- Front-end: `npm install --prefix src/Client && npm run dev --prefix src/Client`
- Tests: `dotnet test Apollo.sln`
- Full stack with deps: `docker-compose up --build` (brings up API, Discord, Service, Postgres, and Redis; uses `.env` for secrets)
- AI-assisted dev: `./start-dev.sh` (builds isolated container with OpenCode; starts Postgres + Redis via `compose.dev.yaml`)

## Coding Style & Naming Conventions

- Follow `.editorconfig`: 2-space indentation, UTF-8, max line length 120, trailing whitespace trimmed.
- Prefer file-scoped namespaces in C#; sort `using` directives with `System` first.
- C# naming: PascalCase for public types/members, camelCase for locals/parameters.
- Suffix async methods with `Async`; suffix DTOs with `DTO`.
- CQRS naming: suffix commands with `Command`, queries with `Query`, handlers with `Handler`.
- Events: suffix with `Event` (e.g., `ToDoCreatedEvent`).
- Sort members: constants, fields, constructors, properties, methods, then by name alphabetically.
- Keep modules thin: Domain for rules, Application for orchestration, API/GRPC/Discord for transport.
- Assign unused variables to `_` to indicate intentional disregard.
- Do not use regions in C#; prefer partial classes if splitting is needed.
- Use primary constructors unless more complex initialization is required.
- Use `sealed` on classes/records not intended for inheritance.
- Use `readonly record struct` for single-value wrappers (e.g., `ToDoId`, `PersonId`).

## Error Handling

- Use FluentResults `Result<T>` and `Result` instead of exceptions for expected failures.
- Return `Result.Ok(value)` for success, `Result.Fail(message)` for failures.
- Use `result.IsFailed` and `result.IsSuccess` for control flow.

## Testing Guidelines

- Framework: xUnit across `tests/` projects; name files `*Tests.cs` and classes `*Tests`.
- Name test methods: `MethodName` + `Scenario` + `ExpectedResult` (e.g., `HandleWithValidInputReturnsSuccessAsync`).
- Use Moq for mocking; use `MockSequence` when verifying ordered interactions.
- Use `WebApplicationFactory` for API integration tests.
- Co-locate fixtures/builders under the relevant test project; stub external services.
- Add regression tests for every bug fix; cover edge cases (null/empty payloads, invalid IDs).
- Run `dotnet test Apollo.sln` before pushing.

## gRPC & Serialization

- Use `[DataContract]` and `[DataMember]` attributes with explicit `Order` for protobuf-net.
- Use `required` properties with `init` setters for required gRPC fields.

## Event Sourcing (Marten)

- Events are immutable records representing facts that occurred.
- Event streams are keyed by aggregate ID (e.g., `ToDoId.Value`).
- Use `StartStream` for new aggregates, `Append` for existing ones.
- Configure inline snapshot projections for read models.

## Commit & Pull Request Guidelines

- Use [Conventional Commits](https://www.conventionalcommits.org/) format: `<emoji> <type>(<scope>): <description>`
- Commit types with emojis:
  | Type | Emoji | Description |
  |------|-------|-------------|
  | `feat` | ✨ | New feature |
  | `fix` | 🐛 | Bug fix |
  | `docs` | 📚 | Documentation |
  | `style` | 💎 | Code style/formatting |
  | `refactor` | ♻️ | Code refactoring |
  | `test` | 🧪 | Tests |
  | `chore` | 🔧 | Maintenance/tooling |
  | `ci` | 👷 | CI/CD changes |
  | `perf` | ⚡ | Performance |
  | `build` | 📦 | Build system |
  | `revert` | ⏪ | Revert changes |
- Optional scope in parentheses: `✨ feat(todos): add reminder scheduling`
- Keep description concise, imperative mood, lowercase
- Examples:
  - `✨ feat: add OpenCode CI workflow`
  - `🐛 fix(database): propagate result failures correctly`
  - `📚 docs: update ARCHITECTURE.md with Mermaid diagrams`
  - `🧪 test(ai): add ToolCallMatchers unit tests`
  - `♻️ refactor(domain): add required modifiers to Person`
- Keep commits focused; include config/docs updates when behavior changes.
- PRs should include: problem/solution summary, linked issue, test evidence, and screenshots for UI changes.
- Update `ARCHITECTURE.md` when endpoints, env vars, or architecture change.

## AI Tooling & Editor Configuration

This project uses **OpenCode** as the AI coding agent and **Zed** as the primary editor.

### OpenCode Setup

- Configuration: `opencode.json` (project root) and `docker/opencode.json` (container)
- Authenticate with `/connect` on first run
- `ARCHITECTURE.md` is auto-loaded as context via the `instructions` config
- C# files are auto-formatted with `dotnet format` after edits

### Custom Commands

| Command | Description |
|---------|-------------|
| `/build` | Build the .NET solution |
| `/test` | Run all tests and analyze failures |
| `/check` | Full pipeline: format, build, test |
| `/review` | Review staged/unstaged changes against coding standards |

### Custom Agents

| Agent | Invoke | Description |
|-------|--------|-------------|
| `plan` | `@plan` | Research-only agent that creates actionable implementation plans |
| `tdd` | `@tdd` | TDD execution agent: writes tests first, then implements |
| `pair-programmer` | `@pair-programmer` | Rapid pair programming: user architects, agent codes with TDD Red-Green |

### Skills (on-demand context)

| Skill | Description |
|-------|-------------|
| `csharp-conventions` | C# naming, typing, and structural conventions |
| `event-sourcing` | Marten event sourcing patterns |
| `cqrs-patterns` | CQRS/MediatR command/query/handler patterns |
| `grpc-contracts` | gRPC/protobuf-net contract conventions |

When you need to search documentation for external libraries, use `context7` MCP tools.

### Zed Editor

- Project settings: `.zed/settings.json`
- Debug configs: `.zed/debug.json` (Apollo.Service, Apollo.API, Apollo.Discord)
- Extensions auto-installed: C#, HTML, Dockerfile, Docker Compose, TOML, Vue
- Zed reads this `AGENTS.md` file as AI assistant rules automatically

## Security & Configuration Tips

- Copy `.env.example` to `.env` and fill secrets locally; never commit real keys or tokens.
- Local services: Postgres (`localhost:5432`) and Redis (`localhost:6379`) via `docker-compose`.
- Validate new endpoints for input validation and logging; avoid leaking sensitive fields.
