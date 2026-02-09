---
name: cqrs-patterns
description: CQRS and MediatR command, query, and handler patterns for the Apollo project
---

## Overview

Apollo uses the CQRS (Command Query Responsibility Segregation) pattern with MediatR for dispatching commands and queries through handlers.

## Architecture Layers

- **Domain** (`Apollo.Domain`) - Core entities, value objects, domain services
- **Application** (`Apollo.Application`) - CQRS use-cases with MediatR commands/queries/handlers
- **Transport** (`Apollo.API`, `Apollo.GRPC`, `Apollo.Discord`) - Thin transport layer, delegates to Application

Keep modules thin: Domain for rules, Application for orchestration, transport layers for serialization/routing only.

## Commands

Commands represent intent to change state. They are processed by command handlers.

- Suffix with `Command`: `CreateToDoCommand`, `UpdatePersonCommand`
- Commands should be records or classes with the data needed to perform the action.
- Command handlers return `Result` or `Result<T>` (FluentResults).

```csharp
public sealed record CreateToDoCommand(string Title, string Description) : IRequest<Result<ToDoId>>;
```

## Queries

Queries represent requests for data. They do not modify state.

- Suffix with `Query`: `GetToDoByIdQuery`, `ListPeopleQuery`
- Query handlers return `Result<T>` with the requested data.

```csharp
public sealed record GetToDoByIdQuery(ToDoId Id) : IRequest<Result<ToDoDTO>>;
```

## Handlers

Handlers process commands or queries.

- Suffix with `Handler`: `CreateToDoCommandHandler`, `GetToDoByIdQueryHandler`
- One handler per command/query.
- Handlers should be `sealed` classes.
- Use constructor injection for dependencies.

```csharp
public sealed class CreateToDoCommandHandler(IStore store)
    : IRequestHandler<CreateToDoCommand, Result<ToDoId>>
{
    public async Task<Result<ToDoId>> Handle(CreateToDoCommand request, CancellationToken ct)
    {
        // Validate, create events, persist through store
    }
}
```

## Testing Handlers

- Use xUnit with Moq for mocking dependencies.
- Test naming: `MethodName` + `Scenario` + `ExpectedResult`
  - e.g., `HandleWithValidInputReturnsSuccessAsync`
  - e.g., `HandleWithInvalidIdReturnsFailureAsync`
- Assert on `result.IsSuccess` / `result.IsFailed`.
- Verify that the correct events were produced for commands.
