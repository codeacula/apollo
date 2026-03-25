---
issue_number: 121
title: "Add persistent reminder follow-up interactions with friendly task aliases"
status: "open"
labels: ["enhancement"]
assignees: []
created_at: "2025-12-30T18:38:54Z"
updated_at: "2026-03-22T19:55:56Z"
source_url: "https://github.com/codeacula/apollo/issues/121"
---

# Summary

Apollo should persist outbound reminder interactions, follow up automatically until the task is resolved or dismissed, and give users a friendly way to reference to-dos/reminders without raw GUIDs.

## Why

- 

## Scope

- 

## Acceptance Criteria

- Apollo persists a reminder interaction whenever it sends a reminder
- Reminder interactions link to the related reminder(s) and/or to-do(s)
- Apollo follows up automatically after 30 minutes by default when unresolved
- Follow-up interval grows exponentially and caps at 7 days
- Explicit replies to Apollo reminder/follow-up messages resolve the correct interaction
- Non-reply messages fall back to the user's latest unresolved reminder interaction
- Per-user follow-up settings exist and default to enabled
- "started" keeps follow-ups active
- "done" resolves linked to-do(s) and stops follow-ups
- "acknowledged" or "dismissed" stops follow-ups
- Users can reference reminder/to-do context through a stored friendly alias rather than raw IDs
- Discord interactions surface the same context in a direct, non-LLM-only way

## Notes

### Goal
If Apollo reminds a user about something and they do not respond, Apollo should follow up automatically. If the user replies to Apollo's reminder or follow-up message, Apollo should treat that as the referenced task context. If there is no explicit reply, Apollo should assume the user is referring to the most recent unresolved reminder-based Apollo message.

This is intended for small-group product viability testing, so behavior should be deterministic, configurable, and annoying by default rather than adaptive.

### Default Behavior
- Follow-ups are enabled by default
- First follow-up happens 30 minutes after the initial reminder if unresolved
- Later follow-ups use capped exponential growth
- Recommended defaults:
  - `InitialFollowUpDelayMinutes = 30`
  - `FollowUpGrowthFactor = 2.0`
  - `MaxFollowUpDelayMinutes = 10080` (7 days)
- Follow-up cadence resets only when a brand new reminder is sent
- If a user says they "started", Apollo should continue follow-ups on the current schedule
- If a user says "done", Apollo should resolve the linked to-do(s) and stop follow-ups
- If a user acknowledges/dismisses the reminder, follow-ups stop
- Users can disable follow-ups per-user, but default should be on

### Friendly References
Users should not need to reference reminders and to-dos by GUID. Apollo should generate a friendly default reference for each relevant item or interaction unless the user explicitly sets a different one.

### Proposed behavior
- Each to-do/reminder/reminder interaction gets a user-facing friendly alias
- Apollo uses that alias in reminder and follow-up messages
- Users can reply using the alias in normal conversation
- Discord interactions/components should also surface and use the alias, so users are not forced to use natural language only
- LLM-generated naming is acceptable as the default path, but it should not be the only path
- Direct component-based or explicit user naming should be able to override the default

### Design notes
- Keep GUIDs internal only
- Use a stable stored alias/reference value for matching and display
- Ensure uniqueness at least per user
- If an LLM default alias is generated, store it once and keep it stable unless the user changes it
- If the LLM path fails, fall back to a deterministic alias generator

### Proposed Design

### 1. Persist reminder interactions
Add a first-class reminder interaction model that links:
- person
- outbound reminder/follow-up send
- reminder ids
- to-do ids
- outbound platform message reference if available
- follow-up status
- follow-up count
- last/next follow-up times
- current delay / cadence state

This should be separate from normal conversation messages.

### 2. Add per-user follow-up preferences
Store follow-up defaults on `Person`, including:
- `FollowUpEnabled`
- `InitialFollowUpDelayMinutes`
- `FollowUpGrowthFactor`
- `MaxFollowUpDelayMinutes`

### 3. Add reply/reference resolution
Extend inbound message handling so Apollo can resolve reminder context using:
1. explicit reply/reference to an Apollo reminder/follow-up message
2. otherwise the most recent unresolved reminder interaction for that user

### 4. Add follow-up scheduling
Add a dedicated follow-up Quartz job that:
- loads the reminder interaction
- checks whether it is still unresolved
- sends a follow-up
- computes the next capped exponential delay
- schedules the next follow-up

### 5. Add response handling
Apollo should interpret follow-up responses like:
- "done" -> complete linked to-do(s), stop follow-ups
- "started" / "working on it" -> mark started, continue follow-ups
- "got it" / "acknowledged" -> stop follow-ups
- "not yet" / "later" -> keep active, do not reset cadence unless explicitly snoozed

### 6. Add friendly aliases and Discord affordances
- add a stored friendly alias/reference to relevant task/reminder/interactions
- use aliases in Apollo-generated reminder/follow-up copy
- support user override via conversation and Discord components
- add Discord interaction affordances so users can resolve, dismiss, or reference the task without relying entirely on freeform language

### Suggested Implementation Order
1. Add per-user follow-up preference fields/events/store methods
2. Add reminder interaction model/events/store
3. Add friendly alias/reference support for reminder-linked task context
4. Persist reminder interactions when reminders are sent
5. Add follow-up scheduler/job
6. Extend inbound message contracts with reply/reference metadata
7. Resolve active reminder interaction in conversation processing
8. Add response handling for done/started/ack/later
9. Expose user settings through plugin/chat flows
10. Add Discord interaction affordances
11. Add tests across store/job/conversation paths

### Deferred / Not In Scope For V1
- adaptive reminder timing
- learned per-user cadence
- quiet hours
- sentiment-aware follow-up logic
- advanced batching/window optimization based on usage analytics
