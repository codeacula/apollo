---
issue_number: 120
title: "Add Recurring Reminders"
status: "open"
labels: ["enhancement"]
assignees: []
milestone: "short-term-recurring-reminders-and-management"
milestone_doc: "[[short-term-recurring-reminders-and-management]]"
created_at: "2025-12-30T17:59:05Z"
updated_at: "2026-01-09T15:33:19Z"
source_url: "https://github.com/codeacula/apollo/issues/120"
---

# Summary

A user may want to keep a long-running to do that never actually gets completed and is instead treated like a recurring reminder. We need the ability to automatically schedule recurring reminders for a ToDo without the user having to provide input for each one.

## Why

- 

## Scope

- 

## Acceptance Criteria

- Update the Reminders to include a frequency time in seconds. This can be used to determine the proper recurring reminder times for each reminder: `reminder_frequency_in_seconds * nth reminder = how far to adjust`
- Create a repository method that provides all recurring reminders that don't have appropriate next reminder times within the next provided period set
- Change the reminder process so that when a reminder is sent, if a recurring frequency is also set, that the reminder is then rescheduled to the next appropriate time
- Create a job that validates reminders in case one isn't properly updated or the system goes down for a period of time. This job should pull all the reminders using the new repository method and automatically schedule the next reminder

## Notes

- Related issues:
- Context:
