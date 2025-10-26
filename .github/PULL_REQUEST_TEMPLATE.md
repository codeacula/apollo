# 📦 Pull Request

## 🧠 Summary

Brief description of the change and its impact. What was done and why? This should give reviewers a quick understanding of the PR's purpose.

## 🔗 Related Issue(s)

Please link to any relevant issues, discussions, or tasks.

- Closes #[issue_number] (if this PR fully resolves the issue)
- Relates to #[issue_number] (if this PR is part of a larger effort or touches upon the issue)

## 🛠️ Implementation Highlights (Optional)

If this PR involves significant architectural changes, introduces new patterns/libraries, or affects core components in a notable way, please provide a brief overview of the technical approach and key decisions made.

- Example: "Introduced a new `NotificationService` using MediatR for domain event handling."
- Example: "Refactored the `CharacterCreationModule` to use the Builder pattern for better flexibility."

## 🧪 How to Test These Changes

Provide clear, step-by-step instructions for reviewers to validate the changes. Include any necessary setup, specific inputs, or expected outcomes.

1. **Setup (if any):** (e.g., "Ensure PostgreSQL and Redis are running.")
2. **Action:** (e.g., "Send a POST request to `/api/campaigns` with the following JSON body: ...")
3. **Verification:** (e.g., "Expect a 201 Created response and verify the new campaign appears in the database.")
   _OR_
   (e.g., "Run the `CampaignCreationTests` unit test suite.")

## ⚠️ Potential Impact & Risks (Optional)

Outline any potential impacts (e.g., performance degradation/improvement, changes to other modules, deployment considerations) or risks reviewers should be particularly aware of.

- If this PR introduces a **breaking change**, clearly describe it here AND ensure it's marked in your commit messages (e.g., `BREAKING CHANGE: The`PlayerId` parameter in `GetPlayerStats` is now a `Guid` instead of an `int`.`).
-

## ✅ Checklist

Please verify the following before requesting a review:

- [ ] My code follows the architecture, patterns, and coding conventions outlined in `CONTRIBUTING.md` and the project's `copilot-instructions.md`.
- [ ] This PR introduces no breaking changes to existing APIs or functionality OR any breaking changes are clearly documented above and in commit messages.
- [ ] I have added or updated unit tests to cover the new logic or fixes.
- [ ] I have performed manual validation of these changes and the steps are described in the "How to Test These Changes" section.
- [ ] I have updated relevant documentation (e.g., code comments, `README.md`, `plan.md`, ADRs) if necessary.
- [ ] All automated checks (build, tests, linters) pass locally.
