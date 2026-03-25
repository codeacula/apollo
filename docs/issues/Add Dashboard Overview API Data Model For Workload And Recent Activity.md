---
issue_number: 210
title: "Add dashboard overview API/data model for workload and recent activity"
status: "open"
labels: ["enhancement"]
assignees: []
created_at: "2026-03-22T20:14:38Z"
updated_at: "2026-03-22T20:14:38Z"
source_url: "https://github.com/codeacula/apollo/issues/210"
---

# Summary

The client dashboard needs a backend-facing overview shape instead of assembling scattered status calls.\n\n## Goal\n\nProvide a single dashboard overview payload that the client can use to render the dashboard MVP.\n\n## Suggested contents\n\n- initialization/readiness state\n- active to-do counts\n- reminder counts (upcoming / due soon / sent recently if available)\n- recent activity summaries\n- basic system/configuration status\n\n## Acceptance Criteria\n\n- a single API endpoint or client-facing query exists for dashboard overview data\n- the payload is stable and intentionally shaped for the dashboard UI\n- the client does not need to stitch together multiple unrelated calls just to render the overview\n- the API shape leaves room for telemetry fields from #202 later\n\n## Notes\n\nThis is the data/backend companion to the first dashboard slice tracked in #95.\n

## Why

- 

## Scope

- 

## Acceptance Criteria

- 

## Notes

- Related issues:
- Context:
