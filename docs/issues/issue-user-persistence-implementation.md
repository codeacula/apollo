# User Aggregate Persistence Implementation Summary

**Date:** December 3, 2025
**Status:** ✅ Complete

## Overview

Implemented event sourcing for User aggregates with PostgreSQL (via Marten) for event storage and read model projections. The system automatically updates view models when user events occur.

## Implementation Details

### 1. Event Store Setup

**Package Added:** Marten 7.36.3 and Marten.AspNetCore 7.36.3

**Configuration:** [ServiceCollectionExtensions.cs](src/Apollo.Database/ServiceCollectionExtensions.cs#L21-L33)

- Event store with GUID-based stream identity
- Inline snapshot projection for `UserReadModel`
- Lightweight sessions for performance

### 2. Domain Events

Created four event types in [Apollo.Domain/Users/Events/](src/Apollo.Domain/Users/Events/):

- **UserCreatedEvent** - Initial user creation with username, display name, and access status
- **UserUpdatedEvent** - Display name changes
- **UserAccessGrantedEvent** - Access permission granted
- **UserAccessRevokedEvent** - Access permission revoked

### 3. User Aggregate Root

**Transformed:** [User.cs](src/Apollo.Domain/Users/Models/User.cs) from simple record to full aggregate root

**Key Features:**

- Event sourcing behavior with `Apply()` methods for each event type
- Uncommitted events collection for unit of work pattern
- Command methods: `Create()`, `UpdateDisplayName()`, `GrantAccess()`, `RevokeAccess()`
- Version tracking for optimistic concurrency
- Rehydration support from event streams

### 4. Read Model & Projection

**UserReadModel:** [UserReadModel.cs](src/Apollo.Database/Repository/UserReadModel.cs)

- Simple DTO for queries
- Properties: Id, Username, DisplayName, HasAccess, CreatedOn, UpdatedOn

**UserProjection:** [UserProjection.cs](src/Apollo.Database/Repository/UserProjection.cs)

- Static `Create()` method for UserCreatedEvent
- Static `Apply()` methods for update events
- Uses Marten's snapshot projection (inline lifecycle)

### 5. Repository Implementation

**IUserRepository:** [IUserRepository.cs](src/Apollo.Database/Repository/IUserRepository.cs)

- `GetAsync()` - Loads aggregate from event stream
- `SaveAsync()` - Appends uncommitted events to stream

**MartenUserRepository:** [MartenUserRepository.cs](src/Apollo.Database/Repository/MartenUserRepository.cs)

- Rehydrates aggregates by replaying events
- Persists new events to event stream
- Clears uncommitted events after save

### 6. Data Access Implementation

**MartenUserDataAccess:** [MartenUserDataAccess.cs](src/Apollo.Database/Repository/MartenUserDataAccess.cs)

- Replaces `MockUserDataAccess`
- Queries `UserReadModel` projection
- Implements `IUserDataAccess.GetUserAccessAsync()`

## Architecture Flow

```
Command → Aggregate Root → Events → Event Store (PostgreSQL)
                                         ↓
                                   Projection
                                         ↓
                                  Read Model (UserReadModel)
                                         ↓
                                   Queries (MartenUserDataAccess)
```

## Event Sourcing Benefits

1. **Full Audit Trail** - Every change is captured as an event
2. **Temporal Queries** - Can reconstruct state at any point in time
3. **Event-Driven Architecture** - Easy to add event handlers/subscribers
4. **Separation of Concerns** - Write model (aggregate) vs read model (projection)
5. **Scalability** - Read and write models can be scaled independently

## Migration Strategy

The current implementation runs **alongside** the existing EF Core setup:

- EF Core still manages the database schema
- Marten uses the same connection string for event store tables
- `MockUserDataAccess` replaced by `MartenUserDataAccess`
- Existing EF Core Users table remains untouched (can be migrated later)

## Next Steps (Future Enhancements)

1. **Event Versioning** - Add version handling for event schema evolution
2. **Async Projections** - Move to async daemon for better performance at scale
3. **Data Migration** - Create command to migrate existing users to event streams
4. **Event Handlers** - Add domain event handlers for cross-aggregate coordination
5. **Snapshots** - Implement snapshots for aggregates with many events
6. **Remove EF Users Table** - Once stable, migrate fully to Marten

## Testing Recommendations

1. Create test for `User.Create()` and verify event generation
2. Test aggregate rehydration from events
3. Test projection updates when events are saved
4. Verify `MartenUserDataAccess` queries work correctly
5. Test optimistic concurrency with version tracking

## Files Created/Modified

### Created (11 files)

- `Apollo.Domain/Users/Events/UserCreatedEvent.cs`
- `Apollo.Domain/Users/Events/UserUpdatedEvent.cs`
- `Apollo.Domain/Users/Events/UserAccessGrantedEvent.cs`
- `Apollo.Domain/Users/Events/UserAccessRevokedEvent.cs`
- `Apollo.Database/Repository/UserReadModel.cs`
- `Apollo.Database/Repository/UserProjection.cs`
- `Apollo.Database/Repository/IUserRepository.cs`
- `Apollo.Database/Repository/MartenUserRepository.cs`
- `Apollo.Database/Repository/MartenUserDataAccess.cs`

### Modified (3 files)

- `Apollo.Database/Apollo.Database.csproj` - Added Marten packages
- `Apollo.Database/ServiceCollectionExtensions.cs` - Configured Marten
- `Apollo.Domain/Users/Models/User.cs` - Transformed to aggregate root

---

**Implementation Status:** All tasks completed successfully with no compilation errors.
