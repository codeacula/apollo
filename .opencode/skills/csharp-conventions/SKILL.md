---
name: csharp-conventions
description: C# naming, typing, and structural conventions for the Apollo project
---

## Naming Conventions

| Element | Convention | Example |
|---------|-----------|---------|
| Public types/members | PascalCase | `PersonService`, `GetNameAsync` |
| Local variables/parameters | camelCase | `personId`, `resultValue` |
| Async methods | Suffix with `Async` | `HandleAsync`, `GetByIdAsync` |
| DTOs | Suffix with `DTO` | `PersonDTO`, `ToDoDTO` |
| Commands | Suffix with `Command` | `CreateToDoCommand` |
| Queries | Suffix with `Query` | `GetToDoByIdQuery` |
| Handlers | Suffix with `Handler` | `CreateToDoCommandHandler` |
| Events | Suffix with `Event` | `ToDoCreatedEvent` |
| ID wrappers | `readonly record struct` | `ToDoId`, `PersonId` |

## Type Design

- Use `sealed` on classes/records not intended for inheritance.
- Use `readonly record struct` for single-value wrappers (e.g., `ToDoId`, `PersonId`).
- Use primary constructors unless complex initialization is required.
- Use file-scoped namespaces in all C# files.
- Do not use regions; prefer partial classes if splitting is needed.

## Member Ordering

Sort members within a type in this order:
1. Constants
2. Fields
3. Constructors
4. Properties
5. Methods

Within each group, sort alphabetically by name.

## Error Handling

- Use FluentResults `Result<T>` and `Result` instead of exceptions for expected failures.
- Return `Result.Ok(value)` for success, `Result.Fail(message)` for failures.
- Use `result.IsFailed` and `result.IsSuccess` for control flow.
- Reserve exceptions for truly exceptional/unexpected situations.

## Imports and Formatting

- Sort `using` directives with `System` first.
- Follow `.editorconfig`: 2-space indentation, UTF-8, max 120 character lines.
- Assign unused variables to `_` to indicate intentional disregard.

## Async Patterns

- Suffix all async methods with `Async`.
- Use `CancellationToken` parameters where appropriate.
- Prefer `await` over `.Result` or `.Wait()`.

## Dependency Injection

- Register services in the appropriate project's DI configuration.
- Use constructor injection via primary constructors.
- Prefer interfaces for service abstractions.
