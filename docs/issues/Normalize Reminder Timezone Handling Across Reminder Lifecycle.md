---
issue_number: null
title: "Normalize reminder timezone handling across reminder lifecycle"
status: "draft"
labels: ["buggums", "planning"]
assignees: []
milestone: "short-term-reminder-trust-and-reliability"
milestone_doc: "[[short-term-reminder-trust-and-reliability]]"
created_at: "2026-03-25T00:00:00Z"
updated_at: "2026-03-25T00:00:00Z"
source_url: ""
---

# Summary

Normalize timezone handling across reminder creation, storage, scheduling, and display so Apollo behaves consistently for the user's local time.

## Why

- The current timezone bug in `docs/issues/ReminderCreatedComponent Needs To Use The User's Timezone.md` is likely part of a broader consistency problem
- Recurring reminders and follow-up reminders will be hard to trust if timezone behavior is inconsistent

## Scope

- Define how reminder times are interpreted at creation time
- Ensure persisted reminder times are stored in a consistent canonical form
- Ensure user-facing reminder confirmation and display use the user's timezone
- Validate timezone behavior for recurrence and follow-up scheduling

## Acceptance Criteria

- Reminder creation uses the user's timezone consistently when turning natural language into scheduled time
- User-facing reminder confirmation components display times in the user's timezone
- Reminder scheduling and recurrence calculations behave correctly across timezone boundaries
- Follow-up reminders preserve correct local-time expectations after the original reminder is sent

## Notes

- This should include or supersede the narrower UI issue in `docs/issues/ReminderCreatedComponent Needs To Use The User's Timezone.md`
