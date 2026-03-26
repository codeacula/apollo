---
title: "Short-Term Milestone 1: Reminder Trust and Reliability"
status: "active"
target_window: "short-term"
order: 1
theme: "reminders"
plan_doc: "[[Plan]]"
---

# Summary

Make reminder behavior trustworthy before adding richer reminder automation. This milestone focuses on correctness, consistency, and shared reminder state rules.

## Goals

- ensure reminder times and displays respect the user's timezone consistently
- fix known delivery behavior that makes reminder output confusing
- define a reminder lifecycle model that follow-ups, recurrence, management flows, and dashboard views can share

## Included Issues

- `docs/issues/ReminderCreatedComponent Needs To Use The User's Timezone.md`
- `docs/issues/Normalize Reminder Timezone Handling Across Reminder Lifecycle.md`
- `docs/issues/Apollo Doesn't Send One Message For Multiple Reminders.md`
- `docs/issues/Define Reminder Lifecycle States And Transitions.md`

## Exit Criteria

- reminder creation, scheduling, and display have a clear timezone contract
- due reminders are delivered in a way that feels intentional rather than noisy or fragmented
- reminder state transitions are documented clearly enough for the next milestones to implement against

## Notes

- This milestone should land before heavier nagging reminder automation
- The lifecycle/state definitions here should be referenced by later reminder issues instead of duplicated
