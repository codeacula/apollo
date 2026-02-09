---
name: event-sourcing
description: Marten event sourcing patterns and conventions for the Apollo project
---

## Overview

Apollo uses Marten for event sourcing with PostgreSQL as the backing store. Events represent immutable facts that have occurred in the system.

## Event Design

- Events are **immutable records** representing facts that occurred.
- Event names use past tense with `Event` suffix: `ToDoCreatedEvent`, `PersonUpdatedEvent`.
- Events should contain only the data needed to reconstruct state.

```csharp
public sealed record ToDoCreatedEvent(string Title, string Description);
```

## Event Streams

- Event streams are keyed by aggregate ID (e.g., `ToDoId.Value`, `PersonId.Value`).
- Use `StartStream` for **new aggregates** (first event in a stream).
- Use `Append` for **existing aggregates** (subsequent events).

```csharp
// New aggregate
session.Events.StartStream(todoId.Value, new ToDoCreatedEvent(...));

// Existing aggregate
session.Events.Append(todoId.Value, new ToDoUpdatedEvent(...));
```

## Projections

- Configure **inline snapshot projections** for read models.
- Inline projections are updated synchronously within the same transaction as the event append.
- Snapshot projections maintain a current-state view of an aggregate.

## Store Abstraction

- Apollo wraps Marten operations in a unified `Store` abstraction.
- Use the CQRS pattern (MediatR) to dispatch commands that produce events.
- Commands validate input, produce events, and persist them through the Store.

## Testing Event-Sourced Aggregates

- Test that commands produce the correct events.
- Test that projections correctly apply events to build read models.
- Test edge cases: duplicate events, out-of-order processing, missing streams.
