# GitHub Copilot - PR Title and Description Generation for Apollo

You are generating the title and description for a Pull Request (PR) in the Apollo project. Your goal is to provide a clear, concise summary that helps reviewers understand the changes by populating the project's PR template.

1. **Primary Guidance**:
   - The general coding standards, architectural principles, and commit message conventions are defined in the main `.github/copilot-instructions.md`. These will inform the content of the PR.
   - The PR description MUST follow the structure of the `.github/PULL_REQUEST_TEMPLATE.md` file. Reproduce all sections from that template.

2. **PR Title Generation**:
   - **Format**: `EMOJI <type>(<scope>): <Concise Subject>`
   - **EMOJI**: Select an appropriate emoji based on the primary `<type>`:
       - `feat`: ✨ | `fix`: 🐛 | `perf`: ⚡️ | `refactor`: ♻️ | `docs`: 📚 | `test`: ✅ | `chore`: 🧹 | `style`: 🎨
       - If multiple types, choose for the most significant.
   - **`<type>`**: Determine the primary Conventional Commit type from the branch's commits.
   - **`<scope>` (Optional)**: Identify the primary area of change.
   - **`<Concise Subject>`**: Brief, imperative summary (max 72 chars). Example: `✨ feat(GameSession): Implement narrative turn processing`

3. **PR Description Generation (Populating `.github/PULL_REQUEST_TEMPLATE.md`)**:
    Analyze the commits in the branch and the code changes to populate each section of the template:

    - **`## 🧠 Summary`**:
        - Provide a 2-4 sentence explanation of what the PR achieves, why it's made, and its impact.
        - Synthesize information from commit messages and the overall diff.

    - **`## 🔗 Related Issue(s)`**:
        - Scan commit messages for keywords like `Closes #<issue_number>`, `Fixes #<issue_number>`, or `Relates to #<issue_number>`.
        - List these clearly. If none found but seems relevant, prompt developer: "Developer: Please link any relevant issues."

    - **`## 🛠️ Implementation Highlights (Optional)`**:
        - Based on the code changes, identify significant architectural decisions, new patterns/libraries introduced, or core components heavily modified.
        - Suggest 1-2 bullet points if applicable. Examples:
            - "Identified new service: `src/Apollo.Application/Services/NewServiceName.cs`."
            - "Major refactoring in `src/Apollo.Domain/Aggregates/SomeAggregate.cs`."
        - For AI-related changes, highlight specific AI model integrations, prompt modifications, or context management improvements. If AI capabilities are expanded, note any new player-facing features these enable.
        - If no major highlights, suggest: "No major architectural highlights for this PR." or leave blank for developer.

    - **`## 🧪 How to Test These Changes`**:
        - **Infer from Tests**: If new unit/integration tests are added, suggest steps related to running them or describe what they cover. (e.g., "Run `NewFeatureTests.cs` to verify X, Y, Z.")
        - **Infer from API Changes**: If API endpoints are added/modified, suggest how to call them (e.g., "Send POST to `/api/new-endpoint` with payload { ... } and expect 200 OK.").
        - **AI Testing Specifics**: For AI component changes, include both deterministic tests (verifying structure and integration) and subjective quality checks (e.g., "Test the NPC conversation with these sample inputs to verify improved coherence"). Note any configuration changes needed for testing AI components locally.
        - **General Logic**: For other changes, try to formulate logical steps based on the code's purpose.
        - **Prompt if Unclear**: If specific steps are hard to infer, suggest: "Developer: Please provide specific manual validation steps for these changes."
        - Always include placeholders for "Setup", "Action", and "Verification".

    - **`## ⚠️ Potential Impact & Risks (Optional)`**:
        - **Scan for Breaking Changes**: Look for `BREAKING CHANGE:` in commit messages. Also, analyze changes to public API signatures, database schemas (if visible), or core configuration.
        - **Performance**: Note if changes involve loops, complex queries, resource-intensive operations, or AI model calls that might impact performance (e.g., response time, memory/token usage, caching requirements).
        - **Dependencies**: Note if new external dependencies are added or existing ones significantly changed.
        - If none obvious, suggest: "No specific impacts or risks identified beyond the described changes." or leave blank for developer.

    - **`## ✅ Checklist`**:
        - Reproduce the full checklist from `.github/PULL_REQUEST_TEMPLATE.md`.
        - Add a note: "Developer: Please verify each item in the checklist before merging."

4. **Overall Tone**:
    - Clear, professional, and helpful.
    - If Copilot cannot confidently fill a section, it should state that and prompt the developer to complete it.
