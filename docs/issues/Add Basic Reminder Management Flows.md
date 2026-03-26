---
issue_number: null
title: "Add basic reminder management flows"
status: "draft"
labels: ["enhancement", "planning"]
assignees: []
milestone: "short-term-recurring-reminders-and-management"
milestone_doc: "[[short-term-recurring-reminders-and-management]]"
created_at: "2026-03-25T00:00:00Z"
updated_at: "2026-03-25T00:00:00Z"
source_url: ""
---

# Summary

Add the core reminder management actions users need after reminders exist: list, reschedule, snooze, and cancel.

## Why

- `docs/Plan.md` calls out basic reminder management, but there is not a dedicated issue for it yet
- Reminder follow-ups and recurring reminders will be frustrating without a simple way to manage active reminders

## Scope

- Add flows for listing active reminders
- Add flows for rescheduling or editing reminder time
- Add flows for snoozing a reminder without rebuilding it from scratch
- Add flows for canceling reminders cleanly

## Acceptance Criteria

- Users can view their active reminders in a direct, non-debug flow
- Users can reschedule an existing reminder to a different time
- Users can snooze an existing reminder for a short follow-up delay
- Users can cancel a reminder and stop future sends for it
- Reminder management behavior works consistently with recurring reminders and follow-up interactions

## Notes

- Keep the first version simple and reliable rather than highly flexible
- This issue should coordinate with recurring reminder and follow-up behavior so state changes stay consistent
