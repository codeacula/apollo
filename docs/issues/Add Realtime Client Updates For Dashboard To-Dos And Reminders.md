---
issue_number: 211
title: "Add realtime client updates for dashboard, to-dos, and reminders"
status: "open"
labels: ["enhancement"]
assignees: []
created_at: "2026-03-22T20:17:42Z"
updated_at: "2026-03-22T20:19:17Z"
source_url: "https://github.com/codeacula/apollo/issues/211"
---

# Summary

The Apollo client should receive live updates when relevant state changes instead of requiring manual refreshes. This is especially important for the dashboard and for usability around task/reminder management.

## Why

- new tasks should appear automatically
- reminders and follow-up state should update without reloads
- the dashboard should feel alive and trustworthy
- realtime updates will make the management UI much more useful during viability testing

## Scope

- add a realtime transport from backend to client using SignalR
- push events for key changes such as:
  - new to-do created
  - to-do updated/completed/deleted
  - reminder created/sent/acknowledged/deleted
  - configuration/setup status changes where relevant
  - dashboard activity updates
- wire the client to update visible state automatically

## Acceptance Criteria

- the client can establish a SignalR connection to Apollo
- key to-do/reminder/dashboard events are pushed to the client
- dashboard data updates automatically when relevant state changes
- task/reminder views no longer depend solely on manual refresh for freshness
- connection failure/degraded mode falls back gracefully to polling/manual refresh

## Notes

- This pairs naturally with #95 (dashboard), #209 (dashboard MVP), and #210 (dashboard overview API/data model).
- For small-group viability testing, a simple and reliable implementation is more important than perfect event coverage on day one.

### Preferred technical direction
Use SignalR as the primary realtime transport.

Why SignalR:
- strong fit for the current .NET stack
- easier client/server development than raw WebSockets
- good fallback/connection management behavior
- clean event-driven model for dashboard and task/reminder updates
