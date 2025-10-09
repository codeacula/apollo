# Apollo, Codeacula's Personal Assistant

## Introduction

Apollo is a personal assistant that is meant to help neurodivergent people with their daily routines. I wrote specifically to help me manage my daily tasks, to check in on me and make sure I'm staying on track, and help me manage my time better. It is not meant to be a full-fledged AI assistant like Siri or Alexa, but rather a tool to help me stay organized and focused.

Explain Apollo's purpose, target audience, and key features here.

## Features

### Planned Features

## Tech Stack

- **Backend**: ASP.NET Core
- **Frontend**: Vue.js
- **Database**: PostgreSQL
- **Caching**: Redis (required for Discord interaction session management)
- **Task Scheduling**: Quartz.NET

## Getting Started

### Prerequisites

- .NET 9.0 SDK
- Docker and Docker Compose
- Node.js (for frontend development)

### Running with Docker Compose

The easiest way to run Apollo is using Docker Compose, which will start all required services (API, PostgreSQL, Redis):

```bash
docker-compose up -d
```

### Local Development

1. Ensure PostgreSQL and Redis are running (via Docker Compose or locally)
2. Update connection strings in `src/Apollo.API/appsettings.Development.json`
3. Run database migrations:
   ```bash
   dotnet ef database update --project src/Apollo.Database
   ```
4. Start the API:
   ```bash
   dotnet run --project src/Apollo.API
   ```

### Redis Configuration

Redis is required for managing Discord interaction sessions. The default configuration expects:
- Host: `localhost:6379`
- Password: `apollo_redis`

Update the `Redis` connection string in `appsettings.Development.json` or set the `REDIS_PASSWORD` environment variable for production deployments.
