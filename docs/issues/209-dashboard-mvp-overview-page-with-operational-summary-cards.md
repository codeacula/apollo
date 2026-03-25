---
issue_number: 209
title: "Dashboard MVP: overview page with operational summary cards"
status: "open"
labels: ["enhancement"]
assignees: []
created_at: "2026-03-22T20:14:37Z"
updated_at: "2026-03-22T20:14:37Z"
source_url: "https://github.com/codeacula/apollo/issues/209"
---

# Summary

Apollo's dashboard needs a first real implementation slice so it becomes useful and motivating instead of just a placeholder.\n\n## Goal\n\nTurn the current dashboard shell into an overview page that gives a quick sense of what Apollo is doing right now.\n\n## Scope\n\n- redesign the dashboard landing page around summary cards/sections\n- show initialization/readiness state prominently\n- show key workload counts such as active to-dos and upcoming reminders\n- show a recent activity section for reminder/conversation events\n- add light live-refresh behavior so the dashboard feels current\n\n## Acceptance Criteria\n\n- dashboard is visually intentional and not a plain status dump\n- a user can quickly tell whether Apollo is configured and active\n- dashboard shows useful top-level counts or summaries for current workload\n- dashboard shows recent Apollo activity in some human-readable form\n- dashboard refreshes often enough to feel alive during local/testing use\n\n## Notes\n\nThis is the first implementation slice of #95. It can use placeholder/mock sections temporarily if needed, but should be wired to real data where available.\n

## Why

- 

## Scope

- 

## Acceptance Criteria

- 

## Notes

- Related issues:
- Context:
