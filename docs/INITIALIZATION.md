# Apollo System Initialization & Configuration Guide

This document provides comprehensive guidance for operators and developers on setting up, configuring, and troubleshooting Apollo's initialization process.

---

## Overview

Apollo's configuration is **event-sourced in PostgreSQL** rather than stored in environment variables. This enables:

- **Mid-application updates** to AI settings, Discord credentials, and super-admin configuration without restarting
- **Immutable audit trail** of all configuration changes
- **Database-backed persistence** for production deployments

The initialization process is **one-time only on first startup**. Subsequent configuration updates use dedicated endpoints.

---

## Operator Setup (Infrastructure-Only)

This section is for deployment operators who need to set up the backing services and infrastructure.

### Prerequisites

- Docker and Docker Compose (for containerized deployment)
- PostgreSQL 16+ and Redis 7+ (if running without Docker)
- Network connectivity for gRPC (port 5270) and HTTP (port 5144)

### 1. Start Backing Services

For Docker deployments, use the provided `docker-compose.yaml`:

```bash
# Start the full stack
docker compose up -d

# Or explicitly start infrastructure only
docker compose up -d pgsql redis
```

Verify services are running:

```bash
# Check container health
docker ps

# PostgreSQL should be on port 5432
# Redis should be on port 6379
```

### 2. Network Configuration

Ensure the following ports are available and configured:

| Service     | Port  | Protocol | Purpose                  |
| ----------- | ----- | -------- | ------------------------ |
| PostgreSQL  | 5432  | TCP      | Event store & documents  |
| Redis       | 6379  | TCP      | Distributed cache        |
| Apollo.Service | 5270 | gRPC    | Backend service          |
| Apollo.API  | 5144  | HTTP     | REST gateway             |

### 3. Connection Strings

Set infrastructure connection strings in your deployment environment (`.env` file or secrets manager):

```bash
# PostgreSQL connection
ConnectionStrings__Apollo="Host=<db-host>;Database=apollo_db;Username=apollo;Password=<password>"

# Redis connection
ConnectionStrings__Redis="<redis-host>:<redis-port>,password=<password>"

# Quartz scheduler connection (shares PostgreSQL)
ConnectionStrings__Quartz="Host=<db-host>;Database=apollo_db;Username=apollo;Password=<password>"
```

### 4. gRPC Service Configuration

Configure the gRPC service endpoints:

```bash
GrpcHostConfig__Host="localhost"           # Service hostname
GrpcHostConfig__Port="5270"               # Service gRPC port
GrpcHostConfig__ApiToken="your-token"     # Optional API token
GrpcHostConfig__ValidateSslCertificate=false  # Set true for production
GrpcHostConfig__UseHttps=false             # Set true for production with TLS
```

---

## First-Run Initialization

This section describes how Apollo is initialized on first startup and how users (or operators) configure the system.

### What Happens on First Startup

1. **Database is empty** — no configuration row exists in `mt_doc_dbconfiguration`
2. **Setup wizard is shown** — in the Vue frontend (if using web UI)
3. **Configuration is collected** — AI provider credentials, Discord bot settings, super-admin user
4. **Events are persisted** — `AiConfigurationUpdatedEvent`, `DiscordConfigurationUpdatedEvent`, `SuperAdminConfigurationUpdatedEvent`
5. **Read models are updated** — inline Marten projections materialize the document
6. **Services initialize with configuration** — Discord bot, AI agent, and notification channels activate

### Using the Setup Wizard (Web UI)

1. **Navigate to the setup page** (when system is not initialized):
   ```
   http://localhost:5144/setup
   ```

2. **Enter AI Configuration**:
   - Model ID (e.g., `gpt-4-turbo-preview`)
   - API Endpoint (e.g., `https://api.openai.com/v1`)
   - API Key (stored encrypted in database)

3. **Enter Discord Configuration**:
   - Bot Token (obtained from Discord Developer Portal)
   - Public Key (for webhook verification)
   - Bot Name (for logging/display)

4. **Designate Super Admin**:
   - Discord user ID of the super-admin
   - This user receives system notifications and has elevated permissions

5. **Submit** — System initializes and becomes operational

### Using the Configuration API

If you prefer CLI or programmatic initialization:

```bash
# 1. Check initialization status
curl http://localhost:5144/api/setup/status

# Response (not initialized):
{
  "isInitialized": false,
  "isAiConfigured": false,
  "isDiscordConfigured": false,
  "isSuperAdminConfigured": false
}

# 2. POST initial setup
curl -X POST http://localhost:5144/api/setup \
  -H "Content-Type: application/json" \
  -d '{
    "aiModelId": "gpt-4-turbo-preview",
    "aiEndpoint": "https://api.openai.com/v1",
    "aiApiKey": "sk-...",
    "discordToken": "MTA0...",
    "discordPublicKey": "pub_...",
    "superAdminDiscordUserId": "123456789"
  }'

# Response (on success):
{
  "message": "Setup completed successfully.",
  "isInitialized": true,
  "isAiConfigured": true,
  "isDiscordConfigured": true,
  "isSuperAdminConfigured": true
}

# 3. Verify initialization
curl http://localhost:5144/api/setup/status
```

---

## Developer Configuration (Local Development)

This section is for developers setting up a local development environment.

### Prerequisites

- .NET 10 SDK
- Node.js 20+ and Bun
- Docker Desktop (for backing services)

### 1. Start Backing Services

```bash
# Start PostgreSQL and Redis in Docker
docker compose up -d pgsql redis

# Verify they're running
docker ps | grep -E "pgsql|redis"
```

### 2. Initialize Database

The database schema is managed via Entity Framework Core migrations. On first run, Marten automatically creates tables.

```bash
# Restore and build (migrations run automatically on startup)
dotnet restore
dotnet build
```

### 3. Run the Services

Open three terminal windows and run each service:

```bash
# Terminal 1: Backend Service (gRPC + Scheduler)
dotnet watch --project src/Apollo.Service/Apollo.Service.csproj

# Terminal 2: HTTP API Gateway
dotnet watch --project src/Apollo.API/Apollo.API.csproj

# Terminal 3: Discord Bot (optional, requires Discord token)
dotnet watch --project src/Apollo.Discord/Apollo.Discord.csproj
```

### 4. First-Time Setup

Once services are running:

1. **Via Web UI**:
   - Open http://localhost:5144
   - Follow the setup wizard
   - Enter dummy/test credentials

2. **Via API**:
   ```bash
   curl -X POST http://localhost:5144/api/setup \
     -H "Content-Type: application/json" \
     -d '{
       "aiModelId": "test-model",
       "aiEndpoint": "http://localhost:8000",
       "aiApiKey": "test-key",
       "discordToken": "MTA0...",
       "discordPublicKey": "pub...",
       "superAdminDiscordUserId": "999999999"
     }'
   ```

### 5. Verify Setup

```bash
curl http://localhost:5144/api/setup/status
# Should return all subsystems as configured
```

---

## Configuration Architecture

### Database vs Environment Variables

| Configuration | Storage | Update Method | Persistence |
| --- | --- | --- | --- |
| **Infrastructure** (DB host, Redis, gRPC endpoints) | Environment variables / `.env` | Server restart | Needed only at deployment |
| **Application** (AI, Discord, SuperAdmin) | PostgreSQL event stream | HTTP/gRPC API | Event-sourced, queryable |

### Event Storage Structure

Configuration is stored as immutable events in the `mt_events` table:

```sql
-- Events stream (source of truth)
SELECT * FROM mt_events
  WHERE stream_id = '00000000-0000-0000-0000-000000000001'
  ORDER BY seq;

-- Output:
-- AiConfigurationUpdatedEvent
-- DiscordConfigurationUpdatedEvent
-- SuperAdminConfigurationUpdatedEvent

-- Read model (materialized via inline projection)
SELECT * FROM mt_doc_dbconfiguration
  WHERE id = '00000000-0000-0000-0000-000000000001';
```

### Configuration Data Model

```csharp
public sealed record ConfigurationData
{
  public Guid Id { get; init; }
  public string? AiModelId { get; init; }
  public string? AiEndpoint { get; init; }
  public string? AiApiKey { get; init; }
  public string? DiscordToken { get; init; }
  public string? DiscordPublicKey { get; init; }
  public string? DiscordBotName { get; init; }
  public string? SuperAdminDiscordUserId { get; init; }
  public string? DefaultTimeZoneId { get; init; }
  public int DefaultDailyTaskCount { get; init; } = 5;

  // Computed properties
  public bool IsAiConfigured => !string.IsNullOrWhiteSpace(AiModelId);
  public bool IsDiscordConfigured => !string.IsNullOrWhiteSpace(DiscordToken);
  public bool IsSuperAdminConfigured => !string.IsNullOrWhiteSpace(SuperAdminDiscordUserId);
  public bool IsInitialized => IsAiConfigured || IsDiscordConfigured || IsSuperAdminConfigured;
}
```

---

## API Reference

### Configuration Endpoints

#### `GET /api/setup/status`

Returns the current initialization and subsystem configuration status.

**Response (200 OK):**

```json
{
  "isInitialized": true,
  "isAiConfigured": true,
  "isDiscordConfigured": true,
  "isSuperAdminConfigured": true
}
```

**Use cases:**
- Check if system is ready
- Display configuration status in admin dashboard
- Determine which setup steps are complete

---

#### `POST /api/setup`

Performs one-time initial system setup. Can only be called when system is **not initialized**.

**Request:**

```json
{
  "aiModelId": "gpt-4-turbo-preview",
  "aiEndpoint": "https://api.openai.com/v1",
  "aiApiKey": "sk-...",
  "discordToken": "MTA0...",
  "discordPublicKey": "pub...",
  "discordBotName": "Apollo",
  "superAdminDiscordUserId": "123456789"
}
```

**Response (200 OK):**

```json
{
  "message": "Setup completed successfully.",
  "isInitialized": true,
  "isAiConfigured": true,
  "isDiscordConfigured": true,
  "isSuperAdminConfigured": true
}
```

**Response (409 Conflict) - Already initialized:**

```json
{
  "error": "System is already initialized. Use dedicated update endpoints to modify configuration."
}
```

**Response (400 Bad Request) - Invalid input:**

```json
{
  "error": "AI configuration failed: At least one of ModelId or Endpoint must be provided."
}
```

---

## Troubleshooting

### Issue: "System is already initialized" on first startup

**Symptom:** POST /api/setup returns 409 Conflict even though this is the first startup.

**Causes:**
- Configuration was already set via environment variables in a previous run
- Database was not cleaned between restarts

**Solutions:**
1. Check existing configuration:
   ```bash
   curl http://localhost:5144/api/setup/status
   ```
2. If partially configured, use dedicated update endpoints instead of POST /setup
3. To reset (development only):
   ```bash
   # Drop and recreate database
   docker compose down -v pgsql
   docker compose up -d pgsql
   ```

---

### Issue: "At least one of ModelId or Endpoint must be provided"

**Symptom:** POST /api/setup fails with 400 Bad Request.

**Cause:** AI configuration requires at least ModelId or Endpoint.

**Solution:** Provide valid AI configuration:
```json
{
  "aiModelId": "gpt-4-turbo-preview",
  "aiEndpoint": "https://api.openai.com/v1",
  "aiApiKey": "sk-..."
}
```

---

### Issue: Configuration not persisting across restarts

**Symptom:** Setup wizard reappears after service restart.

**Cause:** Configuration is stored in PostgreSQL. If database is not persistent:
- Volumes not mounted in Docker
- Database migration failed
- Events not being written

**Solutions:**
1. Verify PostgreSQL volume:
   ```bash
   docker volume ls | grep apollo
   docker inspect <volume-name>
   ```
2. Check database migrations:
   ```bash
   psql postgresql://apollo:apollo@localhost:5432/apollo_db -c "SELECT * FROM mt_events;"
   ```
3. Check Apollo.Service logs for errors during initialization
4. Verify connection string is correct:
   ```bash
   echo $ConnectionStrings__Apollo
   ```

---

### Issue: Discord bot token rejected

**Symptom:** Discord configuration accepted but bot doesn't connect.

**Causes:**
- Token is invalid or revoked
- Bot doesn't have required permissions
- Public key mismatch

**Solutions:**
1. Verify token in Discord Developer Portal: https://discord.com/developers/applications
2. Ensure bot has required intents enabled (Message Content, etc.)
3. Check public key matches:
   ```bash
   curl http://localhost:5144/api/setup/status | grep isDiscordConfigured
   ```
4. Check Apollo.Discord logs for connection errors

---

### Issue: Redis connection errors

**Symptom:** "Cannot connect to Redis" in service logs.

**Cause:** Redis not running or incorrect connection string.

**Solutions:**
1. Verify Redis is running:
   ```bash
   docker ps | grep redis
   redis-cli ping
   ```
2. Check connection string:
   ```bash
   echo $ConnectionStrings__Redis
   ```
3. Restart Redis:
   ```bash
   docker compose restart redis
   ```

---

## Building and Testing

### Frontend (Vue)

The frontend is built with Vite and Bun:

```bash
cd src/Client

# Install dependencies
bun install

# Development server
bun run dev

# Production build
bun run build

# Run tests
bun test
```

### Backend (.NET)

```bash
# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run tests
dotnet test

# Run specific service
dotnet run --project src/Apollo.Service/Apollo.Service.csproj
```

---

## Environment Variables Reference

See `.env.example` in the project root for all available configuration options.

**Key variables:**
- `ConnectionStrings__Apollo` - PostgreSQL connection
- `ConnectionStrings__Redis` - Redis connection
- `GrpcHostConfig__*` - gRPC service configuration
- Application config (AI, Discord, SuperAdmin) is set via `/api/setup` endpoint, not env vars

---

## Further Reading

- **Architecture**: See `ARCHITECTURE.md` for system design and data flow
- **Database**: See `src/Apollo.Database/README.md` for schema details
- **Application Commands**: See `src/Apollo.Application/Configuration/` for command/query handlers
