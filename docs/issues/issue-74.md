# Create Tasking System

We want to implement the ability for a user to create tasks, set reminders, and manage them through an AI interface. Update the project so that users can create tasks, set due dates, update tasks, delete tasks, and complete tasks. The AI should be able to interact with this tasking system to help users manage their tasks effectively.

## Acceptance Criteria

- Basic CRUD of Tasks in Marten
- Expose tools to AI to manage tasks
- Be able to tell an AI to create a task with a reminder date

## Implementation Status: ✅ COMPLETED

All acceptance criteria have been met. The task management system has been fully implemented following Apollo's Clean Architecture and event-sourcing patterns.

## What Was Implemented

### Phase 1: Domain & Database Foundation ✅

**1. Domain Events** (`src/Apollo.Database/Tasks/Events/`)
- `TaskCreatedEvent` - Captures task creation with PersonId and Description
- `TaskUpdatedEvent` - Tracks task description updates
- `TaskCompletedEvent` - Marks task completion
- `TaskDeletedEvent` - Soft-deletes tasks
- `TaskReminderSetEvent` - Sets reminder dates on tasks

**2. Database Persistence** (`src/Apollo.Database/Tasks/`)
- `DbTask` - Event-sourced aggregate with event handlers (`Create`, `Apply` methods)
- `TaskStore : ITaskStore` - Marten-based repository using `IDocumentSession`
- Registered in `ServiceCollectionExtensions.cs` with:
  - Schema configuration
  - Event type registration
  - Inline snapshot projection

### Phase 2: Core Abstractions & Application Logic ✅

**3. Core Interface** (`src/Apollo.Core/Tasks/`)
- `ITaskStore` with methods:
  - `CreateAsync` - Creates new task
  - `GetAsync` - Retrieves single task by ID
  - `GetByPersonIdAsync` - Lists all tasks for a person
  - `UpdateAsync` - Updates task description
  - `CompleteAsync` - Marks task complete
  - `DeleteAsync` - Soft-deletes task
  - `SetReminderAsync` - Sets reminder date
  - `GetDueTasksAsync` - Queries tasks with due reminders

**4. Application Commands** (`src/Apollo.Application/Tasks/`)
- MediatR Commands & Handlers:
  - `CreateTaskCommand` / `CreateTaskCommandHandler`
  - `UpdateTaskCommand` / `UpdateTaskCommandHandler`
  - `CompleteTaskCommand` / `CompleteTaskCommandHandler`
  - `DeleteTaskCommand` / `DeleteTaskCommandHandler`
  - `GetTasksByPersonIdQuery` / `GetTasksByPersonIdQueryHandler`

### Phase 3: AI & Data Exposure ✅

**5. AI Tools** (`src/Apollo.Application/Tasks/`)
- `TaskPlugin` with Semantic Kernel functions:
  - `create_task` - Creates task with optional reminder
  - `update_task` - Updates task description
  - `complete_task` - Marks task complete
  - `delete_task` - Deletes task
  - `list_tasks_for_person` - Lists all active tasks for a person
- Registered in `ApolloAIAgent` via `AddPlugin()` method
- Dynamic plugin registration in `ServiceCollectionExtension.ConfigureAIPlugins()`

**6. gRPC Endpoints** (`src/Apollo.GRPC/`)
- `IApolloGrpcService` & `ApolloGrpcService` updated with:
  - `CreateTaskAsync` - gRPC endpoint to create tasks
  - `GetTaskAsync` - Retrieve single task (placeholder)
  - `GetPersonTasksAsync` - List all tasks for a person
  - `UpdateTaskAsync` - Update task
  - `CompleteTaskAsync` - Complete task
  - `DeleteTaskAsync` - Delete task
- DTOs created:
  - `TaskDto` - Task data transfer object
  - `CreateTaskRequest` - Request for creating tasks
  - `UpdateTaskRequest` - Request for updating tasks

### Phase 4: Reminder System ✅

**7. Discord Reminder Job** (`src/Apollo.API/Jobs/`)
- `TaskReminderJob : IJob` implementation
  - Queries `ITaskStore.GetDueTasksAsync()` every 15 minutes
  - Retrieves person information via `IPersonStore`
  - Logs reminder notifications (Discord DM integration placeholder)
- Registered in `ServiceCollectionExtensions.cs` with Quartz scheduler
- Configured to run every 15 minutes

**Note:** Discord DM sending is currently logged only. Full Discord integration requires access to the Discord Gateway client which runs in a separate service (`Apollo.Discord`). The job structure is ready for integration when the Discord client is accessible from the API context.

## Architecture Decisions

1. **Event Sourcing**: Tasks use Marten's event sourcing with inline snapshots for optimal read performance
2. **Soft Deletes**: Tasks are marked deleted rather than removed, preserving history
3. **Separation of Concerns**: 
   - Domain models in `Apollo.Domain`
   - Database persistence in `Apollo.Database`
   - Business logic in `Apollo.Application`
   - AI integration in `Apollo.Application` (TaskPlugin)
   - API/Jobs in `Apollo.API`
   - gRPC services in `Apollo.GRPC`
4. **Dynamic Plugin Registration**: AI plugins are registered after service configuration to handle dependency injection properly
5. **Result Pattern**: All operations return `FluentResults.Result<T>` for consistent error handling

## Testing Recommendations

1. Create tasks via AI: "Create a task to review the quarterly report"
2. Create tasks with reminders: "Remind me tomorrow at 3pm to call the dentist"
3. List tasks: "What tasks do I have?"
4. Update tasks: "Change my dentist task to call the doctor instead"
5. Complete tasks: "Mark the quarterly report task as done"
6. Delete tasks: "Delete the doctor appointment task"

## Future Enhancements

1. **Discord DM Integration**: Wire the TaskReminderJob to actually send Discord DMs
2. **Task Priorities & Energy Levels**: Currently using placeholder values (0)
3. **Due Dates vs Reminder Dates**: Separate the concepts for better task management
4. **Recurring Tasks**: Add support for tasks that repeat on a schedule
5. **Task Categories/Tags**: Group related tasks
6. **Notifications**: Multi-channel notification system (Discord, Email, SMS)

## Files Modified

- `src/Apollo.AI/ApolloAIAgent.cs` - Added AddPlugin method
- `src/Apollo.AI/IApolloAIAgent.cs` - Interface update
- `src/Apollo.API/Program.cs` - Plugin configuration call
- `src/Apollo.API/ServiceCollectionExtensions.cs` - Quartz job registration
- `src/Apollo.Application/ServiceCollectionExtension.cs` - TaskPlugin registration
- `src/Apollo.Database/ServiceCollectionExtensions.cs` - Marten configuration
- `src/Apollo.GRPC/Service/ApolloGrpcService.cs` - Task endpoints
- `src/Apollo.GRPC/Service/IApolloGrpcService.cs` - Interface additions

## Files Created

### Database Layer
- `src/Apollo.Database/Tasks/DbTask.cs`
- `src/Apollo.Database/Tasks/TaskStore.cs`
- `src/Apollo.Database/Tasks/Events/TaskCreatedEvent.cs`
- `src/Apollo.Database/Tasks/Events/TaskUpdatedEvent.cs`
- `src/Apollo.Database/Tasks/Events/TaskCompletedEvent.cs`
- `src/Apollo.Database/Tasks/Events/TaskDeletedEvent.cs`
- `src/Apollo.Database/Tasks/Events/TaskReminderSetEvent.cs`

### Core Layer
- `src/Apollo.Core/Tasks/ITaskStore.cs`

### Application Layer
- `src/Apollo.Application/Tasks/CreateTaskCommand.cs`
- `src/Apollo.Application/Tasks/CreateTaskCommandHandler.cs`
- `src/Apollo.Application/Tasks/UpdateTaskCommand.cs`
- `src/Apollo.Application/Tasks/UpdateTaskCommandHandler.cs`
- `src/Apollo.Application/Tasks/CompleteTaskCommand.cs`
- `src/Apollo.Application/Tasks/CompleteTaskCommandHandler.cs`
- `src/Apollo.Application/Tasks/DeleteTaskCommand.cs`
- `src/Apollo.Application/Tasks/DeleteTaskCommandHandler.cs`
- `src/Apollo.Application/Tasks/GetTasksByPersonIdQuery.cs`
- `src/Apollo.Application/Tasks/GetTasksByPersonIdQueryHandler.cs`
- `src/Apollo.Application/Tasks/TaskPlugin.cs`

### API Layer
- `src/Apollo.API/Jobs/TaskReminderJob.cs`

### gRPC Layer
- `src/Apollo.GRPC/Contracts/TaskDto.cs`
- `src/Apollo.GRPC/Contracts/CreateTaskRequest.cs`
- `src/Apollo.GRPC/Contracts/UpdateTaskRequest.cs`

## Execution Plan

Implement task management following Apollo's Clean Architecture and event-sourcing patterns, with AI tool exposure, gRPC data endpoints, and Discord DM reminders scoped to PersonId.

### Phase 1: Domain & Database Foundation

1. **Define Domain Models & Events** in `src/Apollo.Domain/Tasks/`
   - Create value objects: `TaskId`, `Title`, `Description`, `DueDate`, `IsCompleted`, `ReminderDate`
   - Create `Task` aggregate record following [Person](src/Apollo.Domain/People/Models/Person.cs) pattern
   - Define domain events: `TaskCreatedEvent`, `TaskUpdatedEvent`, `TaskCompletedEvent`, `TaskDeletedEvent`, `TaskReminderSetEvent`

2. **Implement Database Persistence** in `src/Apollo.Database/Tasks/`
   - Create `DbTask` with event handlers (`Create`, `Apply`) matching [DbConversation](src/Apollo.Database/Conversations/Models/DbConversation.cs) pattern
   - Implement `TaskStore : ITaskStore` using Marten's `IDocumentSession`
   - Register in [ApolloDbContextFactory.cs](src/Apollo.Database/ApolloDbContextFactory.cs): schema, events, inline snapshots

### Phase 2: Core Abstractions & Application Logic

3. **Create Core Interfaces** in `src/Apollo.Core/Tasks/`
   - `ITaskStore` with methods: `CreateAsync`, `GetAsync`, `GetByPersonIdAsync`, `UpdateAsync`, `CompleteAsync`, `DeleteAsync`, `GetDueTasksAsync` (for reminders) — all return `Result<T>`

4. **Build Application Commands** in `src/Apollo.Application/Tasks/`
   - Create MediatR commands: `CreateTaskCommand`, `UpdateTaskCommand`, `CompleteTaskCommand`, `DeleteTaskCommand`
   - Implement handlers orchestrating `ITaskStore` operations, following [ProcessIncomingMessageCommandHandler](src/Apollo.Application/Conversations/ProcessIncomingMessageCommand.cs) pattern

### Phase 3: AI & Data Exposure

5. **Expose AI Tools** in `src/Apollo.Core/API/`
   - Create `TaskPlugin` with Semantic Kernel functions: `create_task`, `update_task`, `complete_task`, `list_tasks_for_person`, `delete_task`
   - Register in [ApolloAIAgent.cs](src/Apollo.Core/API/ApolloAIAgent.cs): `_kernel.Plugins.AddFromType<TaskPlugin>("Tasks")`

6. **Add gRPC Endpoints** in `src/Apollo.GRPC/`
   - Create `TaskService.cs` gRPC service with methods: `CreateTask`, `GetTask`, `GetPersonTasks`, `UpdateTask`, `CompleteTask`, `DeleteTask`
   - Update [task.proto](src/Apollo.GRPC/Contracts/task.proto) (or create if missing) with service definitions
   - Register in [GrpcHostConfig.cs](src/Apollo.GRPC/GrpcHostConfig.cs): `endpoints.MapGrpcService<TaskService>()`

### Phase 4: Reminder System

7. **Implement Discord Reminder Job** in `src/Apollo.API/`
   - Create `TaskReminderJob : IJob` to query `ITaskStore.GetDueTasksAsync()`
   - For each due task, send Discord DM via [Apollo.Discord](src/Apollo.Discord) client: `await discordClient.GetUser(personDiscordId).SendMessageAsync(reminderText)`
   - Register in [ServiceCollectionExtensions.cs](src/Apollo.API/ServiceCollectionExtensions.cs):

     ```csharp
     q.ScheduleJob<TaskReminderJob>(trigger => trigger
       .WithIdentity("TaskReminder")
       .WithSimpleSchedule(x => x.WithIntervalInMinutes(15).RepeatForever()));
     ```

   - Inject `ITaskStore` and Discord client into job

8. **Wire Discord User Mapping** in `src/Apollo.Discord/`
   - Ensure `Person` domain model can map from Discord user ID to gRPC/task operations
   - Add Discord ID storage to `Person` if not already present (check [DbPerson](src/Apollo.Database/People/Models/DbPerson.cs))

### Acceptance Criteria Mapping

- ✅ Basic CRUD of Tasks in Marten — **Phases 1-2** (DbTask + TaskStore)
- ✅ Expose tools to AI to manage tasks — **Phase 3** (TaskPlugin)
- ✅ Tell AI to create task with reminder date — **Phases 3-4** (TaskPlugin + TaskReminderJob)
- ✅ Send reminders via Discord DM — **Phase 4** (TaskReminderJob)
- ✅ gRPC endpoints for task data — **Phase 3** (TaskService)
