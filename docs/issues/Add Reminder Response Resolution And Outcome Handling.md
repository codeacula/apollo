---
issue_number: null
title: "Add reminder response resolution and outcome handling"
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

Teach Apollo how to resolve inbound replies against the correct active reminder interaction and apply reminder outcomes consistently.

## Why

- Follow-up reminders are only useful if Apollo can tell what the user is responding to
- Reminder replies should map cleanly to outcomes like done, started, dismissed, or later

## Scope

- Resolve reminder context from explicit replies to Apollo reminder/follow-up messages
- Fall back to the latest unresolved reminder interaction when explicit reply metadata is absent
- Apply consistent outcome handling for reminder-linked to-dos and follow-up state

## Acceptance Criteria

- Explicit replies to Apollo reminder or follow-up messages resolve the correct reminder interaction
- Non-reply messages can fall back to the latest unresolved reminder interaction for that user
- "done" resolves linked to-do items and stops follow-ups
- "started" keeps the reminder interaction active without resetting cadence
- "acknowledged" or "dismissed" stops follow-ups without completing linked to-dos
- Outcome handling is deterministic enough for non-LLM paths and direct interaction components later

## Notes

- This is a child planning issue for `docs/issues/Add Persistent Reminder Follow-Up Interactions With Friendly Task Aliases.md`
- Friendly aliases and Discord components can build on this after the core reply resolution works
