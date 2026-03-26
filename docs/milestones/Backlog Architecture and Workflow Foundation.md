---
title: "Backlog: Architecture and Workflow Foundation"
status: "planned"
target_window: "backlog"
order: 7
theme: "architecture"
plan_doc: "[[Plan]]"
---

# Summary

Track the architecture and internal workflow improvements that improve maintainability, reduce duplication, and clarify platform behavior.

## Goals

- improve internal workflow correctness and maintainability
- review architecture areas that are likely to cause future friction
- reduce duplication and unclear boundaries in existing implementation paths

## Included Issues

- `docs/issues/Add Versioning To Event Source Workflow.md`
- `docs/issues/Be Able To Create Conversation Summary Checkpoints.md`
- `docs/issues/Figure Out How To Handle Message Edits.md`
- `docs/issues/Review Tool Call Setup For Complexity.md`
- `docs/issues/Store Tool Call Results Add Metadata.md`
- `docs/issues/GetDailyPlanQuery Looks Like It's Duplicating Code.md`
- `docs/issues/Combine Config Settings Into One.md`
- `docs/issues/Review Architecture.md`
- `docs/issues/Investigate If All The gRPC Contracts Have To Explicitly State User Credentials.md`

## Exit Criteria

- core workflow boundaries are clearer and easier to evolve
- known duplication and architecture review items have concrete resolution paths
- the platform has less hidden complexity in event, message, and tool-call handling
