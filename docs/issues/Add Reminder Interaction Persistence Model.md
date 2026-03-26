---
issue_number: null
title: "Add reminder interaction persistence model"
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

Add a first-class reminder interaction record so Apollo can persist outbound reminder sends and track whether a reminder thread is still unresolved.

## Why

- Follow-up reminders need stable state instead of relying on transient conversation context
- Reminder replies should resolve against a durable interaction record
- Dashboard and telemetry work will need a reliable source of reminder interaction history

## Scope

- Add a reminder interaction model tied to person, reminder ids, to-do ids, outbound message reference, and follow-up state
- Persist reminder interactions whenever Apollo sends a reminder
- Store enough cadence state to support future follow-up scheduling without re-deriving context

## Acceptance Criteria

- Apollo creates a reminder interaction whenever it sends a reminder that expects follow-up handling
- Reminder interactions link to the relevant person, reminder ids, and to-do ids
- Reminder interactions can store outbound platform message references when available
- Reminder interactions track unresolved/resolved state plus follow-up count and next follow-up time
- Store/repository support exists to fetch the latest unresolved reminder interaction for a user

## Notes

- This is a child planning issue for `docs/issues/Add Persistent Reminder Follow-Up Interactions With Friendly Task Aliases.md`
- Keep the model separate from normal conversation messages
