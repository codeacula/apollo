# Local Development Guide

This guide explains how to run the Apollo services locally on your machine for development, debugging, and hot reloading, while running the database and cache in Docker containers.

## 1. Prerequisites

Ensure you have the following installed:

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

## 2. Start Backing Services

Instead of running the full stack in Docker, we will only start PostgreSQL and Redis. These will be exposed to `localhost` on their default ports.

```bash
docker-compose up -d pgsql redis
```

Verify they are running:

- **Postgres:** `localhost:5432`
- **Redis:** `localhost:6379`

## 3. Configuration & Secrets

The project is configured to use `appsettings.Development.json` for local defaults (connecting to localhost), but sensitive keys (API Tokens) must be set using **User Secrets** to avoid committing them to git.

### Set up Secrets

Run the following commands in your terminal to set your personal API keys.

#### Apollo.API

```bash
cd src/Apollo.API
dotnet user-secrets set "Discord:Token" "YOUR_DISCORD_BOT_TOKEN"
dotnet user-secrets set "ApolloAIConfig:ApiKey" "YOUR_OPENAI_OR_LLAMA_KEY"
# Optional: Override Model Endpoint if not using local default
# dotnet user-secrets set "ApolloAIConfig:Endpoint" "https://api.openai.com/v1"
```

#### Apollo.Discord

```bash
cd src/Apollo.Discord
dotnet user-secrets set "Discord:Token" "YOUR_DISCORD_BOT_TOKEN"
dotnet user-secrets set "Discord:PublicKey" "YOUR_DISCORD_PUBLIC_KEY"
```

#### Apollo.Service

```bash
cd src/Apollo.Service
dotnet user-secrets set "ApolloAIConfig:ApiKey" "YOUR_OPENAI_OR_LLAMA_KEY"
```

## 4. Running Services Locally

You can run each service in a separate terminal window to enable hot reloading (`dotnet watch`).

### Terminal 1: Apollo Service (Backend Logic)

This is the core logic service.

```bash
dotnet watch --project src/Apollo.Service/Apollo.Service.csproj
```

### Terminal 2: Apollo API (HTTP Endpoints)

This hosts the REST API.

```bash
dotnet watch --project src/Apollo.API/Apollo.API.csproj
```

*Access Swagger UI at: <http://localhost:5144/swagger>*

### Terminal 3: Apollo Discord (Bot)

This runs the Discord bot connection.

```bash
dotnet watch --project src/Apollo.Discord/Apollo.Discord.csproj
```

## Troubleshooting

- **Database Connection Errors:** Ensure the Docker containers are running (`docker ps`) and that port 5432 is not occupied by a locally installed Postgres instance.
- **gRPC Errors:** If the API or Discord bot cannot talk to the Service, ensure `Apollo.Service` is running and listening on port **5270**.
