# Contributing to Rydia

Thanks for helping build Rydia! This guide walks through the essentials for getting a local environment running and contributing code.

## Prerequisites

- .NET 9.0 SDK
- Node.js 20.x (or newer) and npm
- Docker Desktop (for running PostgreSQL locally via compose) 
- PowerShell 5.1+ or compatible shell

## Initial Setup

1. Clone the repository and switch to the branch you are working on.
2. Copy `.env.example` to `.env` (create it if it does not exist) and populate the database credentials:
	```env
	POSTGRES_USER=rydia
	POSTGRES_PASSWORD=rydia
	POSTGRES_DB=rydia_db
	```
3. Restore .NET dependencies:
	```powershell
	cd src/Rydia.API
	dotnet restore
	```
4. Install frontend dependencies:
	```powershell
	cd src/Client
	npm install
	```

## Running the Stack

### Backend API

1. Ensure PostgreSQL is available (see Database section below).
2. From `src/Rydia.API`, run:
	```powershell
	dotnet run
	```
3. The ASP.NET Core backend exposes the REST API and connects to Discord via NetCord when configured.

### Frontend Client

1. From `src/Client`, start the Vite dev server:
	```powershell
	npm run dev
	```
2. Open the displayed local URL in a browser to access the Rydia web interface.

### Docker (Optional All-In-One)

1. From the repository root, start the stack:
	```powershell
	docker compose up --build
	```
2. This spins up PostgreSQL along with the backend and frontend containers.

## Database

- Entity Framework Core migrations live in `src/Rydia.Database/Migrations` and include the Quartz persistent store schema.
- On startup, Rydia automatically applies pending migrations to the configured PostgreSQL database (e.g., from `appsettings.Development.json`).
- For local development, you can run PostgreSQL via Docker:
  ```powershell
  docker compose up postgres
  ```
- The schema is kept current automatically, so a fresh install only needs valid database credentials.

## Testing

- Run backend tests from the solution root:
  ```powershell
  dotnet test
  ```
- Run backend tests with code coverage:
  ```powershell
  dotnet test --collect:"XPlat Code Coverage"
  ```
  Coverage reports are generated in `tests/*/TestResults/*/coverage.cobertura.xml`
- Run frontend checks (when available) from `src/Client`:
  ```powershell
  npm run test
  ```

## Contribution Workflow

1. Create a feature branch from `main` or the appropriate base branch.
2. Follow existing coding styles; keep commits focused and descriptive.
3. Include tests or documentation updates when introducing new functionality.
4. Open a pull request and reference related issues when applicable.
5. Be ready to iterate based on review feedback.

Thanks again for contributing—your help keeps Rydia improving!
