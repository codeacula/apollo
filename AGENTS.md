# Repository Guidelines

## Project Structure & Module Organization

- `src/` holds all runtime code: `Apollo.API` (HTTP host), `Apollo.GRPC` (gRPC host), `Apollo.Discord` (bot), `Apollo.Application` (use-cases), `Apollo.Domain` (entities/value objects), `Apollo.Core` (shared contracts/logging), `Apollo.AI` (AI agents/plugins), `Apollo.Database` (data access), `Apollo.Cache` (Redis helpers), and `Client` (Vite + Vue front-end).
- `tests/` mirrors the runtime projects with xUnit projects named `*.Tests`.
- `assets/` stores shared static files.
- Solution entry point is `Apollo.sln`; environment templates live in `.env.example`.
- See `ARCHITECTURE.md` for comprehensive architecture documentation, coding practices, and local development setup.

## Build, Test, and Development Commands

- Restore/build: `dotnet restore Apollo.sln && dotnet build Apollo.sln`
- Run API locally: `dotnet run --project src/Apollo.API/Apollo.API.csproj`
- Run Discord bot: `dotnet run --project src/Apollo.Discord/Apollo.Discord.csproj`
- Front-end: `npm install --prefix src/Client && npm run dev --prefix src/Client`
- Tests: `dotnet test`
- Full stack with deps: `docker-compose up --build` (brings up Postgres + Redis; uses `.env` for secrets)

## Coding Style & Naming Conventions

- Follow `.editorconfig`: spaces, size 2 (C# files commonly use 4-space indentation via IDE), UTF-8, max line length 120, trailing whitespace trimmed.
- Prefer file-scoped namespaces in C#; sort `using` directives with `System` first.
- C# naming: PascalCase for public types/members, camelCase for locals/parameters, suffix async methods with `Async`, suffix DTOs with `DTO`. Sort members: constants, fields, constructors, properties, methods, then by name alphabetically.
- Keep modules thin: Domain for rules, Application for orchestration, API/GRPC/Discord for transport concerns only.
- Assign unused variables to `_` to indicate intentional disregard.
- Do not use regions in C#; prefer partial classes if splitting is needed.
- Use primary constructors unless more complex initialization is required.

## Testing Guidelines

- Framework: xUnit across `tests/` projects; name files `*Tests.cs` and classes `*Tests`.
- Name test methods using `MethodNameStateUnderTestExpectedBehavior` pattern.
- Co-locate fixtures/builders under the relevant test project; stub external services instead of hitting real APIs.
- Add regression tests for every bug fix and cover edge cases (null/empty payloads, invalid IDs, permission checks).
- Run `dotnet test Apollo.sln` before pushing; include the TRX when CI expects artifacts.

## Commit & Pull Request Guidelines

- Commits in history use short, sentence-case summaries (imperative is preferred), e.g., `Fix ChatMessageDTO name` or `Add ToDo reminder job`.
- Keep commits focused; include config/docs updates when behavior changes.
- PRs should include: problem/solution summary, linked issue, test evidence (`dotnet test` output), and screenshots for Client/UI changes.
- Update `ARCHITECTURE.md` and sample configs when endpoints, env vars, or architecture diagrams change.

## Security & Configuration Tips

- Copy `.env.example` to `.env` and fill secrets locally; never commit real keys or tokens.
- Local services: Postgres (`postgres://apollo:apollo@localhost:5432/apollo_db`) and Redis (password defaults to `apollo_redis`) are provided via `docker-compose`.
- Validate new endpoints for input validation and logging; avoid leaking sensitive fields in logs or responses.
