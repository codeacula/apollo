---
mode: 'edit'
tools: ['codebase']
---
# Starting A Task

## Rules

- Process the prompt to completion
- Only ask the user for feedback if you have questions - otherwise, proceed over each step without additional input

## Prompt

Your goal is to create the initial `task.md` file for the provided task number. Review the history to determine if the task number and details have been provided yet. If you don't know the task number, ask for it before continuing.

Once you have the task number, do the following:

1. Create a new folder in `tasks/` named after the task number: `tasks/{{Task ID}}/`
1. Inside of `tasks/{{Task ID}}/` create a `task.md` file with the following format:

    ```markdown
    # Task {{Task ID}}

    ## Task Details

    {{Task Details}}

    ## Execution Plan

    ## Changelog
    ```

1. If the task details are in the chat history, add the task details to the new `task.md` file.
1. Examine the #codebase and silently create three different plans of executing the task, focusing only on a high-level flow, taking care to follow the architectural and coding standards already established in the [Copilot Instructions](../copilot-instructions.md). Each step should focus on providing the expected tests to ensure the step is complete. The proposed solution should have a clean ingress and egress point.
1. Determine which plan is the most optimal to complete the task, discarding the rest
1. Review each step of the created plan. For each step:
   1. Ensure clean testing guidelines
   1. Ensure it follows previously established patterns
   1. Explain the step to the user, including why it was included in the plan and what it should allow once it's complete
1. Write the complete plan under `## Execution Plan`, using `###` for the heading of each task
1. After you have written the plan, go through it again, this time for each step:
   1. Reviewing the #codebase to determine what code you might want to reuse or interact with.
   1. Create a code plan on how to complete the step. This code plan should detail what files will be worked on, and any appropriate snippets of code
   1. Adding the code plan to the bottom of the current step in a separate `#### Step #: Code Plan` section

## Variable Definitions

- **Task ID**: The number of the task initially given to you.
- **Task Details**: The description of the task pulled from `codeacula/apollo` on GitHub
