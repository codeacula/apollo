# Architecture

Apollo was built to allow independent releases of the Discord bot and API server. This also allows the API to implement load-balancing if needed. I approached development with the concepts of Data- and Domain-Driven Design in mind, focusing on the core business domain and modeling the system around it. The architecture focuses on IoC (Inversion of Control) and DI (Dependency Injection) to achieve loose coupling between components, making the system more maintainable and testable.

## System Overview

Apollo is a distributed system consisting of multiple services and libraries:

- **Apollo.API** - Primary API service with REST endpoints and gRPC server
- **Apollo.Discord** - Discord bot integration service with NetCord
- **Client** - Vue.js frontend application (served via Apollo.API)
- **PostgreSQL** - Primary data store using Entity Framework Core and Marten
- **Redis** - Caching and session management
- **Quartz.NET** - Job scheduling for daily alerts and reminders
- **MediatR** - CQRS pattern implementation for commands and queries
- **Semantic Kernel** - AI orchestration and LLM integration
- **gRPC/protobuf** - High-performance inter-service communication

## Architectural Layers

The codebase follows Clean Architecture with strict dependency rules:

### Domain Layer (Apollo.Domain)

The core business domain with zero external dependencies.

- **Purpose**: Contains pure business entities, value objects, and domain logic
- **Dependencies**: None (self-contained)
- **Key Patterns**:
  - Value Objects for type-safe primitives
  - Domain entities representing core business concepts
  - Enums for domain constants
- **Example Types**:
  - `Person`, `Conversation`, `Message`
  - Value objects: `PersonId`, `Username`, `Content`, `ConversationId`
  - Enums: `Platform`, `Level`

### Core Layer (Apollo.Core)

Defines abstractions and interfaces for infrastructure concerns.

- **Purpose**: Interface definitions for repositories, services, and cross-cutting concerns
- **Dependencies**: `Apollo.Domain`, `FluentResults`, `Microsoft.Extensions.Logging.Abstractions`
- **Key Patterns**:
  - Repository interfaces (`IPersonStore`, `IConversationStore`)
  - Service interfaces (`IPersonService`, `IPersonCache`)
  - API client interfaces (`IApolloAPIClient`)
- **Philosophy**: Enables dependency inversion - application and domain depend on abstractions, not implementations

### Application Layer (Apollo.Application)

Contains application-level business logic and use case orchestration.

- **Purpose**: Implements CQRS commands/queries using MediatR
- **Dependencies**: `Apollo.Core`, `Apollo.Domain`, `Apollo.AI`, `Apollo.Cache`, `MediatR`
- **Key Patterns**:
  - CQRS with MediatR (`IRequest<Result<T>>` commands and handlers)
  - Command/Query separation
  - Result pattern using FluentResults
  - DTOs for data transfer
- **Example**:

  ```csharp
  /// <summary>
  /// Tells the system to process an incoming message from a supported platform.
  /// </summary>
  /// <param name="Message">The message to process.</param>
  /// <seealso cref="ProcessIncomingMessageCommandHandler"/>
  public sealed record ProcessIncomingMessageCommand(NewMessage Message)
    : IRequest<Result<Reply>>;

  internal sealed class ProcessIncomingMessageCommandHandler
    : IRequestHandler<ProcessIncomingMessageCommand, Result<Reply>>
  {
    // Handler implementation
  }
  ```

### Infrastructure Layer

#### Apollo.Database

Implements data persistence using dual persistence strategies.

- **Purpose**: Database access and persistence
- **Dependencies**: `Apollo.Core`, `Apollo.Domain`, Entity Framework Core, Marten, Npgsql
- **Technologies**:
  - **Entity Framework Core**: Settings, migrations, Quartz.NET tables
  - **Marten**: Document store for People and Conversations
- **Key Patterns**:
  - Repository pattern implementation
  - Store pattern for domain aggregates
  - EF Core migrations for schema management
- **Example**: `PersonStore` implements `IPersonStore`

#### Apollo.Cache

Implements caching using Redis.

- **Purpose**: Performance optimization through caching
- **Dependencies**: `Apollo.Core`, `StackExchange.Redis`
- **Technologies**: Redis for distributed caching
- **Key Patterns**:
  - Cache-aside pattern
  - Result pattern for cache operations
- **Example**: `PersonCache` implements `IPersonCache`

#### Apollo.AI

Integrates AI capabilities using Semantic Kernel.

- **Purpose**: AI-powered conversational features
- **Dependencies**: `Microsoft.SemanticKernel`, `FluentResults`
- **Technologies**: Microsoft Semantic Kernel for LLM integration
- **Key Patterns**:
  - Agent pattern (`IApolloAIAgent`)
  - Plugin-based extensibility
  - Configuration-driven prompts

#### Apollo.GRPC

Provides inter-service communication using gRPC.

- **Purpose**: High-performance RPC between services
- **Dependencies**: `Apollo.Core`, `Apollo.Application`, `protobuf-net.Grpc`
- **Technologies**: protobuf-net for contract-first gRPC
- **Key Patterns**:
  - Client/Server pattern
  - Interceptors for cross-cutting concerns
  - Contract-based communication

### Presentation Layer

#### Apollo.API

Primary API service hosting REST endpoints, static files, and gRPC server.

- **Purpose**: Main entry point for API requests
- **Technologies**: ASP.NET Core 10, OpenAPI
- **Features**:
  - REST controllers
  - gRPC server hosting
  - Static file serving (Vue.js frontend)
  - Database migration runner
  - Job scheduling (Quartz.NET)

#### Apollo.Discord

Discord bot integration service.

- **Purpose**: Discord platform integration
- **Technologies**: NetCord (Gateway + Rest APIs)
- **Features**:
  - Slash commands
  - Component interactions (buttons, modals, select menus)
  - Message handling
  - HTTP interactions endpoint (`/interactions`)
- **Key Components**:
  - `IncomingMessageHandler`: Processes Discord messages
  - `DailyAlertSetupComponent`: Configuration UI
  - `RedisDailyAlertSetupSessionStore`: Session management

#### Client (Vue.js)

Frontend web application.

- **Purpose**: Web-based user interface
- **Technologies**: Vue.js, TypeScript, Vite
- **Build**: Compiles to `Apollo.API/wwwroot` for production

## Design Patterns and Practices

### CQRS (Command Query Responsibility Segregation)

Commands and queries are separated:

- **Commands**: Mutate state, return `Result<T>` or `Result`
- **Queries**: Read data, never mutate state
- **Implementation**: MediatR `IRequest<TResponse>` and `IRequestHandler<TRequest, TResponse>`

### Result Pattern

Uses FluentResults library for explicit error handling:

```csharp
// Success
return Result.Ok(value);

// Failure
return Result.Fail("Error message");

// Checking results
if (result.IsSuccess)
{
    var value = result.Value;
}
else
{
    var errors = result.Errors;
}
```

**Benefits**:

- No exceptions for expected failures
- Explicit error handling
- Composable with `.Map()`, `.Bind()`, etc.

### Repository Pattern

Data access abstraction:

- **Interface**: Defined in `Apollo.Core` (e.g., `IPersonStore`)
- **Implementation**: Lives in `Apollo.Database` (e.g., `PersonStore`)
- **Scope**: Per aggregate root
- **Return Type**: Always `Result<T>` for explicit error handling

### Dependency Injection

All services use constructor injection:

- Interfaces registered in `ServiceCollectionExtensions` per project
- Scoped lifetime for most services
- Singleton for configuration and providers

### Error Handling

- **Expected failures**: Result pattern (no exceptions)
- **Unexpected failures**: Exceptions caught at boundaries
- **Logging**: LoggerMessage source generator for high-performance logging

```csharp
[LoggerMessage(Level = LogLevel.Information,
    Message = "Processing message for user {Username}")]
public static partial void ProcessingMessage(
    ILogger logger, string username);
```

### Value Objects

Domain primitives wrapped in strongly-typed value objects:

- **Purpose**: Type safety, validation, domain semantics
- **Examples**: `PersonId`, `Username`, `Content`, `ConversationId`

### DTOs vs Domain Models

- **DTOs**: Immutable records in `Apollo.Application` for external contracts
- **Domain Models**: Rich entities in `Apollo.Domain` with behavior
- **DBOs**: Persistence models used by Marten (document store)

## Technology Stack

### Backend

- **.NET 10**: Target framework
- **ASP.NET Core**: Web framework
- **Entity Framework Core 10**: ORM for relational data
- **Marten 8**: Document database on PostgreSQL
- **MediatR 12**: CQRS implementation (Pinned at version 12 due to licensing)
- **FluentResults 4**: Result pattern library
- **NetCord 1.0**: Discord API integration
- **Quartz.NET 3.15**: Job scheduling
- **Semantic Kernel 1.66**: AI integration
- **StackExchange.Redis 2.10**: Redis client
- **protobuf-net.Grpc**: gRPC implementation

### Frontend

- **Vue.js**: Progressive JavaScript framework
- **TypeScript**: Type-safe JavaScript
- **Vite**: Build tool and dev server

### Infrastructure

- **PostgreSQL**: Primary database
- **Redis 7 Alpine**: Caching and session storage
- **Docker Compose**: Local development orchestration

## Configuration

Configuration uses ASP.NET Core configuration system:

- **appsettings.json**: Base configuration
- **appsettings.Development.json**: Development overrides
- **Environment Variables**: Production configuration
- **User Secrets**: Local development secrets

### Key Configuration Sections

- `ConnectionStrings`: Database connections (Apollo, Redis, Quartz)
- `Discord`: Bot token, public key, configuration
- `GrpcHostConfig`: gRPC host settings
- `ApolloAIConfig`: AI model configuration
- `SuperAdminConfig`: Super admin user setup

## Inter-Service Communication

### Apollo.Discord → Apollo.API

- **Protocol**: gRPC
- **Purpose**: Process messages, execute commands
- **Client**: `ApolloGrpcClient` implements `IApolloAPIClient`
- **Authentication**: API token in metadata

### Client → Apollo.API

- **Protocol**: HTTP/REST
- **Purpose**: Web UI interactions
- **Endpoints**: `/api/*`
- **Format**: JSON

### External → Apollo.Discord

- **Protocol**: HTTP (Discord Interactions)
- **Endpoint**: `/interactions`
- **Purpose**: Discord slash commands and interactions

## Data Flow Example: Processing a Discord Message

1. User sends message in Discord
2. `IncomingMessageHandler` receives message via NetCord Gateway
3. Handler checks user access via `IPersonCache` (Redis)
4. Handler creates gRPC request to Apollo.API
5. `ApolloGrpcService` receives request
6. MediatR dispatches `ProcessIncomingMessageCommand`
7. `ProcessIncomingMessageCommandHandler`:
   - Gets or creates user via `IPersonService`
   - Retrieves conversation via `IConversationStore` (Marten)
   - Adds message to conversation
   - Calls `IApolloAIAgent` for AI response (Semantic Kernel)
   - Returns `Reply` result
8. gRPC response returns to Discord service
9. Handler sends response message to Discord via NetCord

## Testing Strategy

Test projects mirror source structure:

- `Apollo.API.Tests`
- `Apollo.Application.Tests`
- `Apollo.Core.Tests`
- `Apollo.Database.Tests`
- `Apollo.Discord.Tests`
- `Apollo.Domain.Tests`
- `Apollo.GRPC.Tests`

**Testing Patterns**:

- Unit tests for business logic
- Integration tests for database operations
- Mock implementations for external dependencies

## Deployment

### Docker

Multi-stage Dockerfile per service:

- **Base**: Runtime image (mcr.microsoft.com/dotnet/aspnet:10.0)
- **Build**: SDK image for compilation
- **Publish**: Optimized output
- **Final**: Minimal runtime image

### Docker Compose

Local development stack:

```yaml
services:
  apollo-api: Port 5144
  apollo-discord: Port 5145
  postgres: Port 5432
  redis: Port 6379
```

## Security Considerations

- **Input Validation**: At service boundaries
- **Authentication**: Discord bot token, gRPC API token
- **Authorization**: Role-based access control
- **Super Admin**: Configured via username
- **Secrets Management**: User secrets (dev), environment variables (prod)

## Performance Optimizations

- **Caching**: Redis for frequently accessed data (user access)
- **Session Management**: Redis with TTL for temporary state
- **LoggerMessage**: Source-generated logging (zero allocation)
- **Async/Await**: Non-blocking I/O throughout
- **gRPC**: High-performance inter-service communication
- **Document Store**: Marten for schema-less aggregate storage

## Terms

- **Aggregate Root** - A cluster of domain objects treated as a single unit (e.g., `Person`, `Conversation`)
- **Command** - An operation that changes system state, returns `Result<T>`
- **DTO (Data Transfer Object)** - Immutable record for external contracts
- **Handler** - MediatR class implementing `IRequestHandler` for a command or query
- **Query** - A read-only operation that retrieves data without side effects
- **Repository** - Interface for data access, abstracting persistence details
- **Result Pattern** - Explicit success/failure handling without exceptions
- **Service** - Business logic coordinator, orchestrates repositories and other services
- **Store** - Repository implementation for domain aggregates
- **Value Object** - Immutable domain primitive with validation (e.g., `PersonId`, `Username`)
