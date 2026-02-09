---
description: Review recent changes against project coding standards
subtask: true
---
Here are the current changes:

!`git diff --cached`

!`git diff`

Review these changes against the coding standards defined in AGENTS.md. Check for:

- **Naming**: PascalCase for public types/members, camelCase for locals, correct suffixes (Async, Command, Query, Handler, Event, DTO)
- **Structure**: File-scoped namespaces, sealed classes, primary constructors, no regions
- **Error handling**: FluentResults Result<T> instead of exceptions
- **Testing**: Corresponding test coverage for new/changed code
- **gRPC**: Correct DataContract/DataMember attributes with explicit Order
- **Event sourcing**: Immutable event records, correct stream operations

Provide specific, actionable feedback with file paths and line references.
