# Apollo's Architecture Overview

I wrote Apollo as a SaaS-based microservice with a focus on modularity, scalability, and maintainability. The focus was to create a codebase that allowed for rapid development and easy extension of features without compromising the stability of the system. Apollo takes advantage of the following technologies and development practices:

- **gRPC**: The API and Discord bot communicate with the microservice using gRPC calls.
- **CQRS**: Apollo uses Command Query Responsibility Segregation to separate read and write operations, improving scalability and maintainability. We use **MediatR** and **FluentResults** to implement this pattern.
- **Event Sourcing**: The way the user will interact with the system lends itself to using Event Sourcingb as the source-of-truth. We accomplish this using **PostgreSQL** with **Marten**.
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
