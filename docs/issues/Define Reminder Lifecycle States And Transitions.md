---
issue_number: null
title: "Define reminder lifecycle states and transitions"
status: "draft"
labels: ["enhancement", "planning"]
assignees: []
milestone: "short-term-reminder-trust-and-reliability"
milestone_doc: "[[short-term-reminder-trust-and-reliability]]"
created_at: "2026-03-25T00:00:00Z"
updated_at: "2026-03-25T00:00:00Z"
source_url: ""
---

# Summary

Define the state model for reminders and reminder interactions so recurrence, follow-ups, management flows, and dashboard views all use the same lifecycle rules.

## Why

- Several short-term reminder issues depend on a shared understanding of what counts as scheduled, sent, active, snoozed, dismissed, completed, or canceled
- Without explicit lifecycle rules, reminder behavior will drift across jobs, UI, and conversation handling

## Scope

- Define reminder states and reminder interaction states
- Define valid transitions caused by sends, replies, snoozes, recurrence, dismissals, completions, and cancellations
- Clarify which states should appear in dashboard and management views

## Acceptance Criteria

- Reminder lifecycle states are documented clearly enough for implementation work
- Reminder interaction lifecycle states are documented clearly enough for follow-up work
- Expected transitions are defined for recurring reminder sends, follow-up sends, snooze actions, done actions, dismissals, and cancellations
- Downstream reminder issues can reference this state model instead of redefining behavior independently

## Notes

- This is primarily a planning and design issue for the short-term reminder roadmap
