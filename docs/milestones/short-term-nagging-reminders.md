---
title: "Short-Term Milestone 2: Nagging Reminders"
status: "active"
target_window: "short-term"
order: 2
theme: "reminders"
plan_doc: "[[Plan]]"
depends_on_doc: "[[short-term-reminder-trust-and-reliability]]"
---

# Summary

Add persistent reminder follow-up behavior so Apollo keeps unresolved work visible until the user resolves, dismisses, or explicitly acknowledges it.

## Goals

- persist reminder interaction state instead of relying on transient message context
- schedule deterministic follow-ups with per-user preferences
- resolve user replies against the correct active reminder and apply consistent outcomes
- keep friendly aliases and direct interaction affordances as the polish layer after the core loop works

## Included Issues

- `docs/issues/Add Persistent Reminder Follow-Up Interactions With Friendly Task Aliases.md`
- `docs/issues/Add Reminder Interaction Persistence Model.md`
- `docs/issues/Add Reminder Follow-Up Scheduler And Cadence.md`
- `docs/issues/Add Reminder Response Resolution And Outcome Handling.md`

## Exit Criteria

- Apollo can persist reminder interactions and identify unresolved reminder threads
- unresolved reminders can trigger repeat follow-ups on a predictable schedule
- user responses like done, started, or dismissed apply to the correct reminder context

## Notes

- Friendly aliases and Discord interaction affordances are part of this milestone, but should follow the core persistence/scheduler/reply work
- This milestone depends on the reliability and lifecycle work in Milestone 1
