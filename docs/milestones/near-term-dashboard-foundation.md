---
title: "Near-Term Secondary: Dashboard Foundation"
status: "planned"
target_window: "near-term-secondary"
order: 4
theme: "dashboard"
plan_doc: "[[Plan]]"
---

# Summary

Build the first useful dashboard surface so Apollo feels alive during viability testing and exposes enough system state to support iteration.

## Goals

- turn the existing dashboard shell into a real overview page
- shape backend overview data intentionally for the dashboard instead of stitching scattered queries together
- add realtime or near-realtime refresh so reminder and task state feels current
- capture useful telemetry that helps evaluate product behavior during testing

## Included Issues

- `docs/issues/Create Apollo Dashboard For Management And Live Status.md`
- `docs/issues/Dashboard MVP Overview Page With Operational Summary Cards.md`
- `docs/issues/Add Dashboard Overview API Data Model For Workload And Recent Activity.md`
- `docs/issues/Add Realtime Client Updates For Dashboard To-Dos And Reminders.md`
- `docs/issues/Discoverability And Product Telemetry.md`

## Exit Criteria

- the dashboard provides a meaningful operational home screen
- the client has a stable overview payload for dashboard rendering
- key reminder/task changes can appear without manual refresh loops where practical
- telemetry is good enough to understand whether reminder behavior is working during tests

## Notes

- This work remains near-term, but secondary to the reminder roadmap in the current plan
