---
issue_number: 202
title: "Discoverability and product telemetry"
status: "open"
labels: ["enhancement"]
assignees: []
milestone: "near-term-dashboard-foundation"
milestone_doc: "[[near-term-dashboard-foundation]]"
parent_issue: 95
parent_issue_doc: "[[Create Apollo Dashboard For Management And Live Status]]"
created_at: "2026-03-22T19:17:12Z"
updated_at: "2026-03-22T20:01:36Z"
source_url: "https://github.com/codeacula/apollo/issues/202"
---

# Summary

We need better observability into Apollo's behavior so we can evaluate product viability with a small test group.\n\n## Goals\n\n- track operational behavior and failures across reminder, conversation, and AI flows\n- capture usage/engagement signals that help us understand whether features are working\n- avoid overbuilding a custom analytics system when an existing platform (such as PostHog) can cover the basics\n\n## Scope\n\n- structured logs around important workflow boundaries\n- token/cost usage where available from AI calls\n- reminder send / follow-up / acknowledgment / completion metrics\n- feature usage signals for setup, to-dos, reminders, and feedback loops\n- basic product analytics integration (PostHog or equivalent)\n\n## Notes\n\n- This supersedes the narrower PostHog-only issue #116.\n- Keep privacy and small-group testing needs in mind; start simple and useful.\n

## Why

- 

## Scope

- 

## Acceptance Criteria

- 

## Notes

- Related issues:
- Context:
