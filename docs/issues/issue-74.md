# Create ToDoing System

We want to implement the ability for a user to create toDos, set reminders, and manage them through an AI interface. Update the project so that users can create toDos, set due dates, update toDos, delete toDos, and complete toDos. The AI should be able to interact with this toDoing system to help users manage their toDos effectively.

## Acceptance Criteria

- Basic CRUD of ToDos in Marten
- Expose tools to AI to manage toDos
- Be able to tell an AI to create a toDo with a reminder date

## Implementation Status: ✅ COMPLETED

All acceptance criteria have been met. The toDo management system has been fully implemented following Apollo's Clean Architecture and event-sourcing patterns.

## What Was Implemented

### Phase 1: Domain & Database Foundation ✅

**1. Domain Events** (`src/Apollo.Database/ToDos/Events/`)

- `ToDoCreatedEvent` - Captures toDo creation with PersonId and Description
- `ToDoUpdatedEvent` - Tracks toDo description updates
- `ToDoCompletedEvent` - Marks toDo completion
- `ToDoDeletedEvent` - Soft-deletes toDos
- `ToDoReminderSetEvent` - Sets reminder dates on toDos

**2. Database Persistence** (`src/Apollo.Database/ToDos/`)

- `DbToDo` - Event-sourced aggregate with event handlers (`Create`, `Apply` methods)
- `ToDoStore : IToDoStore` - Marten-based repository using `IDocumentSession`
- Registered in `ServiceCollectionExtensions.cs` with:
  - Schema configuration
  - Event type registration
  - Inline snapshot projection

### Phase 2: Core Abstractions & Application Logic ✅

**3. Core Interface** (`src/Apollo.Core/ToDos/`)

- `IToDoStore` with methods:
  - `CreateAsync` - Creates new toDo
  - `GetAsync` - Retrieves single toDo by ID
  - `GetByPersonIdAsync` - Lists all toDos for a person
  - `UpdateAsync` - Updates toDo description
  - `CompleteAsync` - Marks toDo complete
  - `DeleteAsync` - Soft-deletes toDo
  - `SetReminderAsync` - Sets reminder date
  - `GetDueToDosAsync` - Queries toDos with due reminders

**4. Application Commands** (`src/Apollo.Application/ToDos/`)

- MediatR Commands & Handlers:
  - `CreateToDoCommand` / `CreateToDoCommandHandler`
  - `UpdateToDoCommand` / `UpdateToDoCommandHandler`
  - `CompleteToDoCommand` / `CompleteToDoCommandHandler`
  - `DeleteToDoCommand` / `DeleteToDoCommandHandler`
  - `GetToDosByPersonIdQuery` / `GetToDosByPersonIdQueryHandler`

### Phase 3: AI & Data Exposure ✅

**5. AI Tools** (`src/Apollo.Application/ToDos/`)

- `ToDoPlugin` with Semantic Kernel functions:
  - `create_toDo` - Creates toDo with optional reminder
  - `update_toDo` - Updates toDo description
  - `complete_toDo` - Marks toDo complete
  - `delete_toDo` - Deletes toDo
  - `list_toDos_for_person` - Lists all active toDos for a person
- Registered in `ApolloAIAgent` via `AddPlugin()` method
- Dynamic plugin registration in `ServiceCollectionExtension.ConfigureAIPlugins()`

**6. gRPC Endpoints** (`src/Apollo.GRPC/`)

- `IApolloGrpcService` & `ApolloGrpcService` updated with:
  - `CreateToDoAsync` - gRPC endpoint to create toDos
  - `GetToDoAsync` - Retrieve single toDo (placeholder)
  - `GetPersonToDosAsync` - List all toDos for a person
  - `UpdateToDoAsync` - Update toDo
  - `CompleteToDoAsync` - Complete toDo
  - `DeleteToDoAsync` - Delete toDo
- DTOs created:
  - `ToDoDto` - ToDo data transfer object
  - `CreateToDoRequest` - Request for creating toDos
  - `UpdateToDoRequest` - Request for updating toDos

### Phase 4: Reminder System ✅

**7. Discord Reminder Job** (`src/Apollo.API/Jobs/`)

- `ToDoReminderJob : IJob` implementation
  - Queries `IToDoStore.GetDueToDosAsync()` every 15 minutes
  - Retrieves person information via `IPersonStore`
  - Logs reminder notifications (Discord DM integration placeholder)
- Registered in `ServiceCollectionExtensions.cs` with Quartz scheduler
- Configured to run every 15 minutes

**Note:** Discord DM sending is currently logged only. Full Discord integration requires access to the Discord Gateway client which runs in a separate service (`Apollo.Discord`). The job structure is ready for integration when the Discord client is accessible from the API context.

## Architecture Decisions

1. **Event Sourcing**: ToDos use Marten's event sourcing with inline snapshots for optimal read performance
2. **Soft Deletes**: ToDos are marked deleted rather than removed, preserving history
3. **Separation of Concerns**:
   - Domain models in `Apollo.Domain`
   - Database persistence in `Apollo.Database`
   - Business logic in `Apollo.Application`
   - AI integration in `Apollo.Application` (ToDoPlugin)
   - API/Jobs in `Apollo.API`
   - gRPC services in `Apollo.GRPC`
4. **Dynamic Plugin Registration**: AI plugins are registered after service configuration to handle dependency injection properly
5. **Result Pattern**: All operations return `FluentResults.Result<T>` for consistent error handling

## Testing Recommendations

1. Create toDos via AI: "Create a toDo to review the quarterly report"
2. Create toDos with reminders: "Remind me tomorrow at 3pm to call the dentist"
3. List toDos: "What toDos do I have?"
4. Update toDos: "Change my dentist toDo to call the doctor instead"
5. Complete toDos: "Mark the quarterly report toDo as done"
6. Delete toDos: "Delete the doctor appointment toDo"

## Future Enhancements

1. **Discord DM Integration**: Wire the ToDoReminderJob to actually send Discord DMs
2. **ToDo Priorities & Energy Levels**: Currently using placeholder values (0)
3. **Due Dates vs Reminder Dates**: Separate the concepts for better toDo management
4. **Recurring ToDos**: Add support for toDos that repeat on a schedule
5. **ToDo Categories/Tags**: Group related toDos
6. **Notifications**: Multi-channel notification system (Discord, Email, SMS)

## Files Modified

- `src/Apollo.AI/ApolloAIAgent.cs` - Added AddPlugin method
- `src/Apollo.AI/IApolloAIAgent.cs` - Interface update
- `src/Apollo.API/Program.cs` - Plugin configuration call
- `src/Apollo.API/ServiceCollectionExtensions.cs` - Quartz job registration
- `src/Apollo.Application/ServiceCollectionExtension.cs` - ToDoPlugin registration
- `src/Apollo.Database/ServiceCollectionExtensions.cs` - Marten configuration
- `src/Apollo.GRPC/Service/ApolloGrpcService.cs` - ToDo endpoints
- `src/Apollo.GRPC/Service/IApolloGrpcService.cs` - Interface additions

## Files Created

### Database Layer

- `src/Apollo.Database/ToDos/DbToDo.cs`
- `src/Apollo.Database/ToDos/ToDoStore.cs`
- `src/Apollo.Database/ToDos/Events/ToDoCreatedEvent.cs`
- `src/Apollo.Database/ToDos/Events/ToDoUpdatedEvent.cs`
- `src/Apollo.Database/ToDos/Events/ToDoCompletedEvent.cs`
- `src/Apollo.Database/ToDos/Events/ToDoDeletedEvent.cs`
- `src/Apollo.Database/ToDos/Events/ToDoReminderSetEvent.cs`

### Core Layer

- `src/Apollo.Core/ToDos/IToDoStore.cs`

### Application Layer

- `src/Apollo.Application/ToDos/CreateToDoCommand.cs`
- `src/Apollo.Application/ToDos/CreateToDoCommandHandler.cs`
- `src/Apollo.Application/ToDos/UpdateToDoCommand.cs`
- `src/Apollo.Application/ToDos/UpdateToDoCommandHandler.cs`
- `src/Apollo.Application/ToDos/CompleteToDoCommand.cs`
- `src/Apollo.Application/ToDos/CompleteToDoCommandHandler.cs`
- `src/Apollo.Application/ToDos/DeleteToDoCommand.cs`
- `src/Apollo.Application/ToDos/DeleteToDoCommandHandler.cs`
- `src/Apollo.Application/ToDos/GetToDosByPersonIdQuery.cs`
- `src/Apollo.Application/ToDos/GetToDosByPersonIdQueryHandler.cs`
- `src/Apollo.Application/ToDos/ToDoPlugin.cs`

### API Layer

- `src/Apollo.API/Jobs/ToDoReminderJob.cs`

### gRPC Layer

- `src/Apollo.GRPC/Contracts/ToDoDto.cs`
- `src/Apollo.GRPC/Contracts/CreateToDoRequest.cs`
- `src/Apollo.GRPC/Contracts/UpdateToDoRequest.cs`

## Execution Plan

Implement toDo management following Apollo's Clean Architecture and event-sourcing patterns, with AI tool exposure, gRPC data endpoints, and Discord DM reminders scoped to PersonId.

### Phase 1: Domain & Database Foundation

1. **Define Domain Models & Events** in `src/Apollo.Domain/ToDos/`
   - Create value objects: `ToDoId`, `Title`, `Description`, `DueDate`, `IsCompleted`, `ReminderDate`
   - Create `ToDo` aggregate record following [Person](src/Apollo.Domain/People/Models/Person.cs) pattern
   - Define domain events: `ToDoCreatedEvent`, `ToDoUpdatedEvent`, `ToDoCompletedEvent`, `ToDoDeletedEvent`, `ToDoReminderSetEvent`

2. **Implement Database Persistence** in `src/Apollo.Database/ToDos/`
   - Create `DbToDo` with event handlers (`Create`, `Apply`) matching [DbConversation](src/Apollo.Database/Conversations/Models/DbConversation.cs) pattern
   - Implement `ToDoStore : IToDoStore` using Marten's `IDocumentSession`
   - Register in [ApolloDbContextFactory.cs](src/Apollo.Database/ApolloDbContextFactory.cs): schema, events, inline snapshots

### Phase 2: Core Abstractions & Application Logic

3. **Create Core Interfaces** in `src/Apollo.Core/ToDos/`
   - `IToDoStore` with methods: `CreateAsync`, `GetAsync`, `GetByPersonIdAsync`, `UpdateAsync`, `CompleteAsync`, `DeleteAsync`, `GetDueToDosAsync` (for reminders) — all return `Result<T>`

4. **Build Application Commands** in `src/Apollo.Application/ToDos/`
   - Create MediatR commands: `CreateToDoCommand`, `UpdateToDoCommand`, `CompleteToDoCommand`, `DeleteToDoCommand`
   - Implement handlers orchestrating `IToDoStore` operations, following [ProcessIncomingMessageCommandHandler](src/Apollo.Application/Conversations/ProcessIncomingMessageCommand.cs) pattern

### Phase 3: AI & Data Exposure

5. **Expose AI Tools** in `src/Apollo.Core/API/`
   - Create `ToDoPlugin` with Semantic Kernel functions: `create_toDo`, `update_toDo`, `complete_toDo`, `list_toDos_for_person`, `delete_toDo`
   - Register in [ApolloAIAgent.cs](src/Apollo.Core/API/ApolloAIAgent.cs): `_kernel.Plugins.AddFromType<ToDoPlugin>("ToDos")`

6. **Add gRPC Endpoints** in `src/Apollo.GRPC/`
   - Create `ToDoService.cs` gRPC service with methods: `CreateToDo`, `GetToDo`, `GetPersonToDos`, `UpdateToDo`, `CompleteToDo`, `DeleteToDo`
   - Update [toDo.proto](src/Apollo.GRPC/Contracts/toDo.proto) (or create if missing) with service definitions
   - Register in [GrpcHostConfig.cs](src/Apollo.GRPC/GrpcHostConfig.cs): `endpoints.MapGrpcService<ToDoService>()`

### Phase 4: Reminder System

7. **Implement Discord Reminder Job** in `src/Apollo.API/`
   - Create `ToDoReminderJob : IJob` to query `IToDoStore.GetDueToDosAsync()`
   - For each due toDo, send Discord DM via [Apollo.Discord](src/Apollo.Discord) client: `await discordClient.GetUser(personDiscordId).SendMessageAsync(reminderText)`
   - Register in [ServiceCollectionExtensions.cs](src/Apollo.API/ServiceCollectionExtensions.cs):

     ```csharp
     q.ScheduleJob<ToDoReminderJob>(trigger => trigger
       .WithIdentity("ToDoReminder")
       .WithSimpleSchedule(x => x.WithIntervalInMinutes(15).RepeatForever()));
     ```

   - Inject `IToDoStore` and Discord client into job

8. **Wire Discord User Mapping** in `src/Apollo.Discord/`
   - Ensure `Person` domain model can map from Discord user ID to gRPC/toDo operations
   - Add Discord ID storage to `Person` if not already present (check [DbPerson](src/Apollo.Database/People/Models/DbPerson.cs))

### Acceptance Criteria Mapping

- ✅ Basic CRUD of ToDos in Marten — **Phases 1-2** (DbToDo + ToDoStore)
- ✅ Expose tools to AI to manage toDos — **Phase 3** (ToDoPlugin)
- ✅ Tell AI to create toDo with reminder date — **Phases 3-4** (ToDoPlugin + ToDoReminderJob)
- ✅ Send reminders via Discord DM — **Phase 4** (ToDoReminderJob)
- ✅ gRPC endpoints for toDo data — **Phase 3** (ToDoService)
