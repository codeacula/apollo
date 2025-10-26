---
applyTo: "**/*"
---
# General Coding Instructions

## Fundamental Software Design Principles

- **DRY (Don't Repeat Yourself)**:
  - Strive to eliminate redundancy.
  - Extract shared logic into reusable services, helper methods, or extension methods as appropriate for the context.
- **KISS (Keep It Simple, Stupid)**:
  - Favor simple, straightforward solutions over complex ones.
  - Avoid over-engineering; start with the simplest approach that effectively solves the problem.
- **YAGNI (You Ain't Gonna Need It)**:
  - Implement only the functionality that is actively required by current user stories or tasks.
  - Defer adding features or capabilities based on future speculation until they are actually needed.
- **SOLID Principles**:
  - Adhere to SOLID principles to create maintainable and scalable software:
    - **S**ingle Responsibility Principle (SRP)
    - **O**pen/Closed Principle (OCP)
    - **L**iskov Substitution Principle (LSP)
    - **I**nterface Segregation Principle (ISP)
    - **D**ependency Inversion Principle (DIP) (Note: Core architectural layering based on DIP is in `copilot-instructions.md`).

## Common Design Patterns & Practices

- **Behavioral Rules for DDD & CQRS/ES Constructs**:
  - When working with Domain-Driven Design (DDD) aggregates: Ensure aggregates are responsible for guarding their own invariants.
  - For Command Query Responsibility Segregation (CQRS):
    - Commands are operations that mutate state and should typically indicate only success or failure, without returning substantial data.
    - Queries are operations that retrieve data and must not alter state.
  - (Note: The overall adoption and high-level architecture of DDD/CQRS/ES is defined in the main `copilot-instructions.md`).
- **Object Creation Patterns**:
  - Consider using the **Factory** or **Builder** pattern for the construction of complex aggregate roots or value objects, especially if creation logic is non-trivial or needs to be centralized.
- **Data Access Patterns**:
  - Employ the **Repository** pattern for each aggregate root to abstract data persistence concerns. Repositories should expose methods for retrieving and persisting aggregates.
  - Consider the **Specification** pattern for defining complex query criteria in a reusable and composable way, particularly for queries against repositories.
- **General Documentation Practices**:
  - Document significant architectural or structural decisions by creating or updating Architecture Decision Records (ADRs) in the `docs/adr/` directory.
  - For non-obvious logic or complex algorithms within any code, provide clear comments explaining the "why" and "how."

## Error Handling Patterns

- **Result Pattern (Conceptual)**:
  - Use a Result pattern for operations that can fail in expected ways
  - Return success with the operation's value for successful operations
  - Return failure with descriptive error information for expected failure cases
  - Chain operations with Results where appropriate
  - Do not throw exceptions for expected failure scenarios; use the Result pattern instead
- **Option Pattern (Conceptual)**:
  - Use an Option pattern for values that may or may not exist
  - Never return or accept null where an Option would be more appropriate
  - Always handle both the "Some" and "None" cases when consuming optional values

  (Note: See language-specific instruction files for implementation details)

## AI Integration Guidelines

- **Semantic Kernel Integration**:
  - Structure AI prompts as reusable templates with clearly defined variables
  - Separate prompt templates from execution logic to allow for easier testing and updates
  - Cache AI responses when appropriate to reduce API calls and improve performance
  - Include appropriate error handling for AI service failures or unexpected responses
- **AI Context Management**:
  - Maintain clear boundaries between game state, player input, and AI context
  - Implement context windowing strategies for long-running AI interactions
  - Track token usage and implement strategies to handle context limitations

## Domain Event Conventions

- **Event Structure (Conceptual)**:
  - Name events in past tense (e.g., `PlayerJoinedGame`, `CharacterCreated`)
  - Include the aggregate ID and other essential data needed to understand what happened
  - Make events immutable and serializable
  - Include a timestamp and version information
- **Event Versioning**:
  - Use explicit versioning for events to support schema evolution
  - Implement event upgraders for migrating older event versions to current schemas
  - Ensure backward compatibility where possible

## Security Considerations

- **Input Validation**:
  - Validate all user input at the boundary of the system
  - Sanitize potentially dangerous input, especially content that might be processed by AI components
  - Implement appropriate rate limiting and size restrictions
- **Authentication & Authorization**:
  - Clearly separate authentication (who you are) from authorization (what you can do)
  - Implement the principle of least privilege for all operations
  - Use claims-based authorization for fine-grained access control

## General Performance Considerations

- **Be Mindful of Resource Usage**:
  - Write efficient code, especially in frequently executed paths or loops.
  - Consider the performance implications of data structures and algorithms chosen.
- **Caching Strategies (General Principle)**:
  - Identify and consider caching for frequently accessed data that changes infrequently to improve performance and reduce load on underlying systems.
  - (Note: Specific architectural caching solutions like Redis for read models are detailed in `copilot-instructions.md` or relevant infrastructure docs).

---

## Instructions for Copilot

- When generating code, apply these general principles and patterns where appropriate
- If a more specific instruction exists in a language-specific file (e.g., csharp.instructions.md) or the main .github/copilot-instructions.md, that instruction takes precedence for its context
- Favor clarity and maintainability in all generated code
