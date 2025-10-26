# GitHub Copilot - Code Review Guidelines for Apollo

You are assisting with a code review for the Apollo project. Your goal is to identify deviations from best practices, architectural principles, and coding standards, then suggest improvements.

1. **Primary Guidance**: Your most comprehensive set of instructions is in `.github/copilot-instructions.md`. You MUST thoroughly understand and use this document as the basis for your review. Pay attention to ALL sections, as they define the expected standards for this repository.

2. **Core Review Focus (Based on Main Instructions)**:
    When reviewing code, critically assess against these key areas detailed in `.github/copilot-instructions.md`:
    - **Core Architectural & Design Principles**:
        - Verify strict Clean Architecture layer adherence (no outward dependencies from Domain/Application).
        - Check CQRS command/query naming and handler structure.
        - Ensure Event Sourcing principles are applied correctly for relevant entities.
        - Confirm proper use of `Result<T>` and `Option<T>`.
    - **C# Language & Coding Conventions**:
        - Ensure use of modern C# (records, file-scoped namespaces, etc.).
        - Check for XML documentation on public APIs.
        - Verify logging uses source-generated `LoggerMessage`.
        - Promote immutability.
    - **Testing**:
        - Confirm new logic is adequately covered by unit tests (AAA pattern, naming conventions).
        - Ensure tests are in the correct projects.
    - **Error Handling Patterns**:
        - Verify proper use of `Result<T>` pattern instead of exceptions for expected failure cases.
        - Ensure error propagation is explicit and well-documented.
        - Check that optional data uses `Option<T>` rather than null.
    - **AI Integration**:
        - Ensure Semantic Kernel integration follows established patterns.
        - Verify AI components are properly abstracted and testable.
        - Check that AI interactions are properly cached when appropriate.
    - **Security Concerns**:
        - Check for proper input validation, especially for player-provided content.
        - Verify authentication/authorization is correctly applied.
        - Ensure sensitive data is properly protected.
    - **What Copilot Must Avoid**:
        - Actively look for and flag any practices listed in the "Critical Must Avoids" section (e.g., hard-coded config, direct `DateTime.Now` usage, God objects).

3. **Providing Feedback**:
    - **Explain Clearly**: For each suggestion, first explain the *reason* behind it, referencing the specific principle or convention from `.github/copilot-instructions.md` if applicable.
    - **Show Diff**: Provide a concise `diff` snippet showing the suggested change:

      ```diff
      - // Old problematic code
      + // *New* improved code
      ```

    - **Be Constructive**: Frame suggestions to help improve code quality and maintainability.

4. **Output Format for Suggestions**:
    Please format your review comments as follows:

    ```markdown
      ### Suggestion 1: [Brief Title of Suggestion]

      **Reason**: [Explain why this change is recommended, referencing .github/copilot-instructions.md if relevant. For example: "Violates Clean Architecture: Domain layer should not reference Infrastructure."]

      **Current Code Snippet**:
        ```csharp
        // Show 1-3 lines of the existing code with the issue
        ```

      **Suggested Change**:

        ```diff
        - // old code line(s)
        + // new code line(s)
        ```
    ```

    (Repeat for each suggestion)

5. **Developer Workflow Reminders (for the human developer)**:
    While you, Copilot, focus on the code itself, remind the developer to:
    - Run `dotnet format` and any static analyzers locally.
    - Ensure all unit tests pass before and after applying suggestions.
    - Keep the overall pull request size manageable (e.g., target diff ≤500 lines unless justified).
    - Consider requesting Agent Mode for comprehensive automated fixes rather than manual implementation of suggestions.

Your primary role is to help maintain the high quality and architectural integrity of the Apollo codebase by meticulously applying the rules in `.github/copilot-instructions.md`.
