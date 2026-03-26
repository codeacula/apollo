---
issue_number: 95
title: "Create Apollo dashboard for management and live status"
status: "open"
labels: ["enhancement"]
assignees: []
milestone: "near-term-dashboard-foundation"
milestone_doc: "[[near-term-dashboard-foundation]]"
child_issues:
  - 209
  - 210
  - 211
  - 202
child_issue_docs:
  - "[[Dashboard MVP Overview Page With Operational Summary Cards]]"
  - "[[Add Dashboard Overview API Data Model For Workload And Recent Activity]]"
  - "[[Add Realtime Client Updates For Dashboard To-Dos And Reminders]]"
  - "[[Discoverability And Product Telemetry]]"
created_at: "2025-12-17T15:19:59Z"
updated_at: "2026-03-22T20:17:43Z"
source_url: "https://github.com/codeacula/apollo/issues/95"
---

# Summary

Apollo needs a dashboard that makes the system feel alive and lets us see what is going on during small-group product viability testing. This should become a near-term priority because setup is already UI-driven and a strong dashboard will improve both usability and motivation to keep using Apollo.

## Why

- 

## Scope

- 

## Acceptance Criteria

- 

## Notes

- This should work alongside #202 (Discoverability and product telemetry) rather than replacing it.
- The dashboard should become the primary place to visualize telemetry once that data exists.
- We should preserve room for more direct reminder/task management inside this UI over time.
- The first concrete implementation target is the Dashboard overview MVP.

### Goals
- provide a central UI to inspect and manage Apollo state
- surface live or near-live operational visibility into reminders, conversations, people, and system health
- make Apollo feel delightful and intentional, not just functional

### Current starting point
The client already has a minimal `DashboardView` plus configuration status UI, but it is only a shell. The first real slice should turn it into an actual operational home screen.

### Phased Plan

### Slice 1: Dashboard overview MVP
Create a real dashboard landing page that shows:
- initialization / readiness state
- high-level workload counts (active to-dos, upcoming reminders, etc.)
- recent activity or recent reminder/conversation events
- enough live refresh behavior to feel current

This should be the first implementation target.

### Slice 2: Telemetry and operational panels
Layer in richer telemetry from #202 so the dashboard can show:
- reminder sends / follow-ups / acknowledgments
- AI usage and failures where available
- system health and integration readiness

### Slice 3: Management surfaces
Add direct management UX for:
- people
- reminders
- to-dos
- configuration and settings

### Initial dashboard scope
- setup / initialization status
- people overview
- active to-dos and reminders
- recent reminder sends and follow-up status
- recent conversations / message activity
- system health and configuration readiness
- useful telemetry panels driven by issue #202

### Product goals
- improve trust by showing what Apollo is doing
- make debugging easier during viability testing
- create a UI that is actually enjoyable to look at and use
