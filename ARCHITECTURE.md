# Apollo's Architecture Overview

I wrote Apollo as a SaaS-based microservice with a focus on modularity, scalability, and maintainability. The focus was to create a codebase that allowed for rapid development and easy extension of features without compromising the stability of the system. Apollo takes advantage of the following technologies and development practices:

- **gRPC**: The API and Discord bot communicate with the microservice using gRPC calls.
- **CQRS**: Apollo uses Command Query Responsibility Segregation to separate read and write operations, improving scalability and maintainability. We use **MediatR** and **FluentResults** to implement this pattern.
- **Event Sourcing**: The way the user will interact with the system lends itself to using Event Sourcing as the source-of-truth. We accomplish this using **PostgreSQL** with **Marten**.
- **Dependency Injection**: Apollo uses dependency injection to manage dependencies between components, making the system more modular and easier to test.
- **Semantic Kernel**: We use Microsoft's Semantic Kernel to set up tooling and work with LLM providers.

## Environment Variables

The file `.env.example` contains everything you need to setup to run Apollo locally. Copy this file to `.env` and fill in the required values. Reference the Twitch and Discord Developer Portals in order to obtain the necessary API keys and credentials.

## Architectural Overview

The solution is broken up into the following projects to help maintain modularity and provide a clear separation of concerns.

### Apollo.Domain

The `Apollo.Domain` project contains the core domain entities, value objects, and domain services. It represents the business logic and rules of the system, independent of any external dependencies or infrastructure concerns. The domain is organized around key aggregates including `Conversations`, `People`, and `ToDos`.

---

### Apollo.Core

The `Apollo.Core` project contains shared utilities, abstractions, and common functionality that can be used across other projects in the solution. This includes logging utilities, result extensions, time provider helpers, and common DTOs/contracts for API responses, notifications, and data operations. It defines shared abstractions for `Conversations`, `People`, and `ToDos` that other layers implement.

**Notable NuGet Packages:**

- **FluentResults** - Provides a result pattern for error handling without exceptions

---

### Apollo.Application

The `Apollo.Application` project implements the application layer containing use-cases and business orchestration logic. It follows the CQRS pattern using MediatR for command/query handling. This layer coordinates between the domain, AI, cache, and infrastructure layers to fulfill business operations for `Conversations`, `People`, and `ToDos`.

**Notable NuGet Packages:**

- **MediatR** - Mediator pattern implementation for CQRS commands and queries

---

### Apollo.Database

The `Apollo.Database` project handles all data persistence concerns. It provides both traditional Entity Framework Core support and event sourcing capabilities via Marten. Contains the `ApolloDbContext`, migrations, and repository implementations for `Conversations`, `People`, and `ToDos`.

**Notable NuGet Packages:**

- **Marten** - Document database and event store for PostgreSQL (Event Sourcing)
- **Microsoft.EntityFrameworkCore** - Core EF functionality
- **Npgsql.EntityFrameworkCore.PostgreSQL** - PostgreSQL provider for EF Core
- **Microsoft.EntityFrameworkCore.Tools** - EF Core CLI tools for migrations

---

### Apollo.Cache

The `Apollo.Cache` project provides Redis-based caching infrastructure for the application. It contains helpers and service extensions for distributed caching operations.

**Notable NuGet Packages:**

- **StackExchange.Redis** - High-performance Redis client

---

### Apollo.AI

The `Apollo.AI` project contains AI agent implementations powered by Microsoft Semantic Kernel. It provides the `IApolloAIAgent` interface for chat completions, plugin management, and LLM interactions. Includes extensible plugins (e.g., `TimePlugin`) and prompt management for AI-powered features.

**Notable NuGet Packages:**

- **Microsoft.SemanticKernel** - Microsoft's SDK for AI orchestration and LLM integration
- **FluentResults** - Result pattern for error handling

---

### Apollo.GRPC

The `Apollo.GRPC` project provides gRPC communication infrastructure for inter-service communication. It contains both server-side services (`ApolloGrpcService`) and client-side stubs, along with contracts and interceptors. Supports gRPC services for `Conversations` and `People` management.

**Notable NuGet Packages:**

- **Grpc.Net.Client** - gRPC client for .NET
- **protobuf-net.Grpc** - Code-first gRPC implementation
- **FluentResults** - Result pattern for error handling

---

### Apollo.Notifications

The `Apollo.Notifications` project handles outbound notifications to users. It provides a `PersonNotificationClient` abstraction with implementations including `DiscordNotificationChannel` for Discord DM notifications and `NoOpPersonNotificationClient` for testing/disabled scenarios.

**Notable NuGet Packages:**

- **NetCord** - Modern Discord API wrapper
- **FluentResults** - Result pattern for error handling

---

### Apollo.Service

The `Apollo.Service` project is the main background worker/host application. It orchestrates the database, cache, AI, notifications, and gRPC server components. Contains scheduled jobs using Quartz (e.g., `ToDoReminderJob` for ToDo reminders) and serves as the central processing hub.

**Notable NuGet Packages:**

- **Quartz** - Enterprise job scheduler
- **MediatR** - Mediator pattern for CQRS
- **Grpc.AspNetCore** - gRPC server hosting
- **NetCord** - Discord API client

---

### Apollo.API

The `Apollo.API` project is the HTTP/REST API host. It provides OpenAPI documentation, serves static files for the Client SPA, and acts as a lightweight gateway that communicates with the Service layer via gRPC.

**Notable NuGet Packages:**

- **Microsoft.AspNetCore.OpenApi** - OpenAPI/Swagger support
- **StackExchange.Redis** - Redis client for caching

---

### Apollo.Discord

The `Apollo.Discord` project is the Discord bot host application. It handles Discord interactions, slash commands, and bot functionality using NetCord. Communicates with the backend via gRPC and uses the Application layer for business logic.

**Notable NuGet Packages:**

- **NetCord** - Modern, high-performance Discord API wrapper

---

### Client

The `Client` project is a Vue 3 single-page application (SPA) built with Vite and TypeScript. It provides the web-based user interface for Apollo.

**Notable npm Packages:**

- **Vue** - Progressive JavaScript framework
- **Vite** - Next-generation frontend build tool
- **TypeScript** - Typed JavaScript

---

## Coding Practices

### General

- Use DTOs for records exchanged between services to ensure clear contracts and separation of concerns.
- Follow `.editorconfig` settings: 2-space indentation, UTF-8, max 120 character line length, trim trailing whitespace.
- Prefer file-scoped namespaces (`namespace X;`) over block-scoped namespaces.
- Sort `using` directives with `System` namespaces first, then separate import groups.

### Naming Conventions

- **Classes/Records**: PascalCase (e.g., `ToDoStore`, `PersonNotificationClient`)
- **Interfaces**: Prefix with `I` (e.g., `IToDoStore`, `IApolloAIAgent`)
- **Value Objects**: Named after the concept they represent (e.g., `ToDoId`, `Description`, `Priority`)
- **DTOs**: Suffix with `DTO` (e.g., `ToDoDTO`, `ChatCompletionRequestDTO`)
- **Commands/Queries**: Suffix with `Command` or `Query` (e.g., `CreateToDoCommand`, `GetToDoByIdQuery`)
- **Handlers**: Suffix with `Handler` matching the command/query (e.g., `CreateToDoCommandHandler`)
- **Events**: Suffix with `Event` (e.g., `ToDoCreatedEvent`, `AccessGrantedEvent`)

### Type Design

- Use `sealed` on classes and records that are not intended for inheritance.
- Use `readonly record struct` for simple value objects wrapping a single value (e.g., `ToDoId`, `PersonId`).
- Use `record` for domain models with multiple properties.
- Use primary constructors for dependency injection in classes and handlers.
- Mark all nullable reference types explicitly with `?`.

### CQRS Pattern

- Commands represent actions that change state (e.g., `CreateToDoCommand`, `DeleteToDoCommand`).
- Queries represent read operations (e.g., `GetToDoByIdQuery`, `GetToDosByPersonIdQuery`).
- Both commands and queries implement `IRequest<Result<T>>` or `IRequest<Result>` from MediatR.
- Handlers implement `IRequestHandler<TRequest, TResult>` and are named to match their request.

### Error Handling

- Use FluentResults `Result<T>` and `Result` types instead of throwing exceptions for expected failures.
- Return `Result.Ok(value)` for success and `Result.Fail(message)` for failures.
- Wrap unexpected exceptions in try/catch blocks and return `Result.Fail(ex.Message)`.
- Use `result.IsFailed` and `result.IsSuccess` for control flow.
- Use extension methods like `GetErrorMessages()` for formatting error output.

### Async Patterns

- All async methods should accept an optional `CancellationToken` parameter with a default value.
- Suffix async methods with `Async` (e.g., `CreateAsync`, `GetByPersonIdAsync`).
- Use `Task<Result<T>>` as return types for async operations that can fail.

### Logging

- Use source-generated logging with `[LoggerMessage]` attribute for high-performance structured logging.
- Group related log messages in static partial classes (e.g., `ToDoLogs`).
- Include Event IDs for each log message for easier filtering and monitoring.

### Dependency Injection

- Register services in `ServiceCollectionExtension` classes within each project.
- Use extension methods on `IServiceCollection` for modular service registration.
- Use `TryAdd*` methods when providing default implementations that can be overridden.
- Prefer `AddScoped` for request-scoped services, `AddSingleton` for configuration, and `AddTransient` for stateless utilities.

### Testing

- Use xUnit as the testing framework.
- Name test classes with `Tests` suffix matching the class under test (e.g., `CreateToDoCommandHandlerTests`).
- Name test methods descriptively: `MethodName` + `Scenario` + `ExpectedResult` + `Async` suffix (e.g., `HandleWithReminderDateSchedulesJobAndPersistsJobIdAsync`).
- Use Moq for mocking dependencies.
- Use `MockSequence` when verifying ordered interactions.
- Organize tests with Arrange/Act/Assert pattern.

### gRPC Contracts

- Use `[DataContract]` and `[DataMember]` attributes for protobuf-net serialization.
- Use `required` properties with `init` setters for required fields.
- Order `[DataMember]` attributes explicitly with `Order` parameter.

### Event Sourcing

- Events are immutable records representing facts that occurred.
- Event streams are keyed by aggregate ID (e.g., `ToDoId.Value`).
- Use Marten's `StartStream` for new aggregates and `Append` for existing ones.
- Configure inline snapshot projections for read model updates.

---

## Local Development

This section explains how to run the Apollo services locally on your machine for development, debugging, and hot reloading, while running the database and cache in Docker containers.

### Prerequisites

Ensure you have the following installed:

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

### Start Backing Services

Instead of running the full stack in Docker, we will only start PostgreSQL and Redis. These will be exposed to `localhost` on their default ports.

```bash
docker-compose up -d pgsql redis
```

Verify they are running:

- **Postgres:** `localhost:5432`
- **Redis:** `localhost:6379`

### Configuration & Secrets

The project is configured to use `appsettings.Development.json` for local defaults (connecting to localhost), but sensitive keys (API Tokens) must be set using **User Secrets** to avoid committing them to git.

#### Set up Secrets

Run the following commands in your terminal to set your personal API keys.

**Apollo.API:**

```bash
cd src/Apollo.API
dotnet user-secrets set "Discord:Token" "YOUR_DISCORD_BOT_TOKEN"
dotnet user-secrets set "ApolloAIConfig:ApiKey" "YOUR_OPENAI_OR_LLAMA_KEY"
# Optional: Override Model Endpoint if not using local default
# dotnet user-secrets set "ApolloAIConfig:Endpoint" "https://api.openai.com/v1"
```

**Apollo.Discord:**

```bash
cd src/Apollo.Discord
dotnet user-secrets set "Discord:Token" "YOUR_DISCORD_BOT_TOKEN"
dotnet user-secrets set "Discord:PublicKey" "YOUR_DISCORD_PUBLIC_KEY"
```

**Apollo.Service:**

```bash
cd src/Apollo.Service
dotnet user-secrets set "ApolloAIConfig:ApiKey" "YOUR_OPENAI_OR_LLAMA_KEY"
```

### Running Services Locally

You can run each service in a separate terminal window to enable hot reloading (`dotnet watch`).

**Terminal 1: Apollo Service (Backend Logic)**

This is the core logic service.

```bash
dotnet watch --project src/Apollo.Service/Apollo.Service.csproj
```

**Terminal 2: Apollo API (HTTP Endpoints)**

This hosts the REST API.

```bash
dotnet watch --project src/Apollo.API/Apollo.API.csproj
```

*Access Swagger UI at: <http://localhost:5144/swagger>*

**Terminal 3: Apollo Discord (Bot)**

This runs the Discord bot connection.

```bash
dotnet watch --project src/Apollo.Discord/Apollo.Discord.csproj
```

### Troubleshooting

- **Database Connection Errors:** Ensure the Docker containers are running (`docker ps`) and that port 5432 is not occupied by a locally installed Postgres instance.
- **gRPC Errors:** If the API or Discord bot cannot talk to the Service, ensure `Apollo.Service` is running and listening on port **5270**.
