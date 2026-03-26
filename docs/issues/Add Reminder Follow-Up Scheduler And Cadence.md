---
issue_number: null
title: "Add reminder follow-up scheduler and cadence"
status: "draft"
labels: ["enhancement", "planning"]
assignees: []
milestone: "short-term-nagging-reminders"
milestone_doc: "[[short-term-nagging-reminders]]"
parent_issue: 121
parent_issue_doc: "[[Add Persistent Reminder Follow-Up Interactions With Friendly Task Aliases]]"
created_at: "2026-03-25T00:00:00Z"
updated_at: "2026-03-25T00:00:00Z"
source_url: ""
---

# Summary

Add reminder follow-up scheduling so unresolved reminders continue to surface until the user resolves or dismisses them.

## Why

- Apollo's short-term plan explicitly depends on nagging reminders
- Follow-up behavior should be deterministic and configurable during viability testing

## Scope

- Add follow-up scheduling based on persisted reminder interactions
- Use default follow-up cadence settings with capped exponential backoff
- Stop scheduling when the reminder interaction is resolved or follow-ups are disabled

## Acceptance Criteria

- Apollo schedules a first follow-up 30 minutes after an unresolved reminder by default
- Follow-up delay grows exponentially and caps at 7 days by default
- Per-user follow-up preference fields exist for enabled state, initial delay, growth factor, and max delay
- Follow-up jobs skip resolved interactions and do not send duplicate follow-ups
- Follow-up cadence state is updated after each send so the next follow-up can be scheduled correctly

## Notes

- This is a child planning issue for `docs/issues/Add Persistent Reminder Follow-Up Interactions With Friendly Task Aliases.md`
- Implement after reminder interaction persistence exists
